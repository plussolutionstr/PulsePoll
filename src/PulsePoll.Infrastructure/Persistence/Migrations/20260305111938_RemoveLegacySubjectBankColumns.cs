using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacySubjectBankColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_subjects_banks",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "ix_subjects_bank_id",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "bank_id",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "iban",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "iban_full_name",
                table: "subjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bank_id",
                table: "subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "iban",
                table: "subjects",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "iban_full_name",
                table: "subjects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_bank_id",
                table: "subjects",
                column: "bank_id");

            migrationBuilder.AddForeignKey(
                name: "fk_subjects_banks",
                table: "subjects",
                column: "bank_id",
                principalTable: "banks",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
