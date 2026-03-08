using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "distribution_end_hour",
                table: "projects",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "distribution_start_hour",
                table: "projects",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "is_scheduled_distribution",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_notified_at",
                table: "project_assignments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "distribution_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    run_date = table.Column<DateOnly>(type: "date", nullable: false),
                    run_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    scheduled_before = table.Column<int>(type: "integer", nullable: false),
                    distributed_count = table.Column<int>(type: "integer", nullable: false),
                    daily_quota = table.Column<int>(type: "integer", nullable: false),
                    hourly_quota = table.Column<int>(type: "integer", nullable: false),
                    remaining_days = table.Column<int>(type: "integer", nullable: false),
                    is_last_day_flush = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_distribution_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_distribution_logs_projects",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_project_assignments_project_status",
                table: "project_assignments",
                columns: new[] { "project_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_distribution_logs_project_date",
                table: "distribution_logs",
                columns: new[] { "project_id", "run_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "distribution_logs");

            migrationBuilder.DropIndex(
                name: "idx_project_assignments_project_status",
                table: "project_assignments");

            migrationBuilder.DropColumn(
                name: "distribution_end_hour",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "distribution_start_hour",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_scheduled_distribution",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "scheduled_notified_at",
                table: "project_assignments");
        }
    }
}
