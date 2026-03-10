using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBilledFlagToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "billed_at",
                table: "projects",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "billed_by",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_billed",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billed_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "billed_by",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_billed",
                table: "projects");
        }
    }
}
