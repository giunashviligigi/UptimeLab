using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UptimeLab.Api.Data;

#nullable disable

namespace UptimeLab.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240602000000_AddMonitoringFeatures")]
    public partial class AddMonitoringFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WebhookAlertsEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "Users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "MonitoredSites",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorMessage",
                table: "MonitoredSites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastNotifiedStatus",
                table: "MonitoredSites",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WebhookAlertsEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "WebhookUrl", table: "Users");
            migrationBuilder.DropColumn(name: "IsPaused", table: "MonitoredSites");
            migrationBuilder.DropColumn(name: "LastErrorMessage", table: "MonitoredSites");
            migrationBuilder.DropColumn(name: "LastNotifiedStatus", table: "MonitoredSites");
        }
    }
}
