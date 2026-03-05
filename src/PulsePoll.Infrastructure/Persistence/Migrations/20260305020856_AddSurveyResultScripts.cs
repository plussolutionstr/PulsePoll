using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyResultScripts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "survey_result_script_id",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "survey_result_scripts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_result_scripts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "survey_result_patterns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    survey_result_script_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    match_pattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_result_patterns", x => x.id);
                    table.ForeignKey(
                        name: "fk_survey_result_patterns_script",
                        column: x => x.survey_result_script_id,
                        principalTable: "survey_result_scripts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_projects_survey_result_script_id",
                table: "projects",
                column: "survey_result_script_id");

            migrationBuilder.CreateIndex(
                name: "idx_survey_result_patterns_script_order",
                table: "survey_result_patterns",
                columns: new[] { "survey_result_script_id", "order" });

            migrationBuilder.CreateIndex(
                name: "uq_survey_result_scripts_name",
                table: "survey_result_scripts",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_projects_survey_result_script",
                table: "projects",
                column: "survey_result_script_id",
                principalTable: "survey_result_scripts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_projects_survey_result_script",
                table: "projects");

            migrationBuilder.DropTable(
                name: "survey_result_patterns");

            migrationBuilder.DropTable(
                name: "survey_result_scripts");

            migrationBuilder.DropIndex(
                name: "ix_projects_survey_result_script_id",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "survey_result_script_id",
                table: "projects");
        }
    }
}
