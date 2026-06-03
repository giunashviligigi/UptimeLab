using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using UptimeLab.Api.BackgroundServices;
using UptimeLab.Api.Data;
using UptimeLab.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (basic structured console logging) ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=localhost;Port=5432;Database=uptimelab;Username=uptime;Password=uptime_secret";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- HTTP client for uptime checks (15s timeout) ---
builder.Services.AddHttpClient("uptime-checker", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("UptimeLab-Monitor/1.0");
});

builder.Services.AddHttpClient("webhook", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// --- Application services ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IWebsiteChecker, WebsiteChecker>();
builder.Services.AddHostedService<SiteMonitoringWorker>();

// --- JWT authentication ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_ME_use_a_long_random_secret_in_production";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "UptimeLab",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "UptimeLab",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Admin-ready: protect future admin routes with [Authorize(Roles = "Admin")]
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Frontend expects camelCase JSON (token, userId, etc.)
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UptimeLab API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply migrations on startup (DevOps-friendly for Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var pending = db.Database.GetPendingMigrations().ToList();
    if (pending.Count > 0)
        logger.LogInformation("Applying migrations: {Migrations}", string.Join(", ", pending));
    db.Database.Migrate();
    logger.LogInformation("Database ready");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// Prometheus metrics endpoint (Grafana-ready)
app.UseHttpMetrics();
app.MapMetrics();

app.MapControllers();

// Health check for load balancers / Docker / Nginx (includes DB)
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
            return Results.Json(new { status = "unhealthy", reason = "database" }, statusCode: 503);

        return Results.Ok(new
        {
            status = "healthy",
            service = "UptimeLab.Api",
            database = "connected",
            timestamp = DateTime.UtcNow
        });
    }
    catch
    {
        return Results.Json(new { status = "unhealthy", reason = "database" }, statusCode: 503);
    }
});

app.Run();
