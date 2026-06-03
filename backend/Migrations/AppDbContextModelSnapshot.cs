using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UptimeLab.Api.Data;

#nullable disable

namespace UptimeLab.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UptimeLab.Api.Models.CheckResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CheckedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<int?>("HttpStatusCode")
                        .HasColumnType("integer");

                    b.Property<Guid>("MonitoredSiteId")
                        .HasColumnType("uuid");

                    b.Property<int>("ResponseTimeMs")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MonitoredSiteId", "CheckedAt");

                    b.ToTable("CheckResults");
                });

            modelBuilder.Entity("UptimeLab.Api.Models.MonitoredSite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastCheckedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("LastHttpStatusCode")
                        .HasColumnType("integer");

                    b.Property<int?>("LastResponseTimeMs")
                        .HasColumnType("integer");

                    b.Property<int>("LastStatus")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("MonitoredSites");
                });

            modelBuilder.Entity("UptimeLab.Api.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UptimeLab.Api.Models.CheckResult", b =>
                {
                    b.HasOne("UptimeLab.Api.Models.MonitoredSite", "MonitoredSite")
                        .WithMany("CheckResults")
                        .HasForeignKey("MonitoredSiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MonitoredSite");
                });

            modelBuilder.Entity("UptimeLab.Api.Models.MonitoredSite", b =>
                {
                    b.HasOne("UptimeLab.Api.Models.User", "User")
                        .WithMany("MonitoredSites")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("UptimeLab.Api.Models.MonitoredSite", b =>
                {
                    b.Navigation("CheckResults");
                });

            modelBuilder.Entity("UptimeLab.Api.Models.User", b =>
                {
                    b.Navigation("MonitoredSites");
                });
#pragma warning restore 612, 618
        }
    }
}
