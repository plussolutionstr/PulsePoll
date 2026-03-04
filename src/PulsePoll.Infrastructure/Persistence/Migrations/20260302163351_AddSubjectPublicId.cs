using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                table: "subjects",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "idx_subjects_public_id",
                table: "subjects",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_subjects_public_id",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "subjects");
        }
    }
}
