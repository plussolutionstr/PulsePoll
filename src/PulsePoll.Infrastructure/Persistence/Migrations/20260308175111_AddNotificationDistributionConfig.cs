using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDistributionConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_distribution_configs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    hourly_limit = table.Column<int>(type: "integer", nullable: false, defaultValue: 300),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_distribution_configs", x => x.id);
                    table.CheckConstraint("ck_notification_distribution_configs_singleton", "id = 1");
                });

            migrationBuilder.InsertData(
                table: "notification_distribution_configs",
                columns: new[] { "id", "hourly_limit", "created_by", "created_at" },
                values: new object[] { 1, 300, 0, new DateTime(2026, 3, 8, 17, 51, 11, DateTimeKind.Unspecified) });

            // Mevcut NotStarted atamalara ScheduledNotifiedAt set et — tekrar bildirim gitmesin
            migrationBuilder.Sql("""
                UPDATE project_assignments
                SET scheduled_notified_at = NOW()
                WHERE status = 0
                  AND scheduled_notified_at IS NULL
                  AND deleted_at IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_distribution_configs");
        }
    }
}
