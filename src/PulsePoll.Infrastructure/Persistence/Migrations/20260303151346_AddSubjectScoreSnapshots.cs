using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectScoreSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subject_score_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    star = table.Column<int>(type: "integer", nullable: false),
                    core_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    activity_multiplier = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    total_assignments = table.Column<int>(type: "integer", nullable: false),
                    started = table.Column<int>(type: "integer", nullable: false),
                    completed = table.Column<int>(type: "integer", nullable: false),
                    not_started = table.Column<int>(type: "integer", nullable: false),
                    partial = table.Column<int>(type: "integer", nullable: false),
                    disqualify = table.Column<int>(type: "integer", nullable: false),
                    screen_out = table.Column<int>(type: "integer", nullable: false),
                    quota_full = table.Column<int>(type: "integer", nullable: false),
                    reward_approved = table.Column<int>(type: "integer", nullable: false),
                    reward_rejected = table.Column<int>(type: "integer", nullable: false),
                    median_completion_minutes = table.Column<int>(type: "integer", nullable: true),
                    active_days30 = table.Column<int>(type: "integer", nullable: true),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_score_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_subject_score_snapshots_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_subject_score_snapshots_subject_id",
                table: "subject_score_snapshots",
                column: "subject_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subject_score_snapshots");
        }
    }
}
