using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialDaysCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "special_days",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("pk_special_days", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_special_days_date",
                table: "special_days",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "uq_special_days_event_code_date",
                table: "special_days",
                columns: new[] { "event_code", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "special_days");
        }
    }
}
