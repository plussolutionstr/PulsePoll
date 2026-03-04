using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectAppActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subject_app_activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    platform = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    app_version = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    device_id_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_app_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_subject_app_activities_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_subject_app_activities_subject_id_occurred_at",
                table: "subject_app_activities",
                columns: new[] { "subject_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subject_app_activities");
        }
    }
}
