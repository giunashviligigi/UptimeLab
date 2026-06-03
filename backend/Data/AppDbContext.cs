using Microsoft.EntityFrameworkCore;
using UptimeLab.Api.Models;

namespace UptimeLab.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<MonitoredSite> MonitoredSites => Set<MonitoredSite>();
    public DbSet<CheckResult> CheckResults => Set<CheckResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.DisplayName).HasMaxLength(128);
            e.Property(x => x.Role).HasMaxLength(32);
        });

        modelBuilder.Entity<MonitoredSite>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Url).HasMaxLength(2048);
            e.Property(x => x.Name).HasMaxLength(256);
            e.HasOne(x => x.User)
                .WithMany(u => u.MonitoredSites)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CheckResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.MonitoredSiteId, x.CheckedAt });
            e.HasOne(x => x.MonitoredSite)
                .WithMany(s => s.CheckResults)
                .HasForeignKey(x => x.MonitoredSiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
