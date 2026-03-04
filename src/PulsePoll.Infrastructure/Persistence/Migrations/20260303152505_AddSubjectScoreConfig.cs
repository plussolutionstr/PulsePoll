using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectScoreConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subject_score_configs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    participation_weight = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    completion_weight = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    quality_weight = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    approval_trust_weight = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    speed_weight = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    confidence_pivot = table.Column<int>(type: "integer", nullable: false),
                    score_baseline = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    star1max = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    star2max = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    star3max = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    star4max = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    very_active_last_seen_days = table.Column<int>(type: "integer", nullable: false),
                    active_last_seen_days = table.Column<int>(type: "integer", nullable: false),
                    warm_last_seen_days = table.Column<int>(type: "integer", nullable: false),
                    cooling_last_seen_days = table.Column<int>(type: "integer", nullable: false),
                    very_active_min_days30 = table.Column<int>(type: "integer", nullable: false),
                    very_active_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    active_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    warm_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    cooling_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    dormant_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    no_telemetry_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_score_configs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subject_score_configs");
        }
    }
}
