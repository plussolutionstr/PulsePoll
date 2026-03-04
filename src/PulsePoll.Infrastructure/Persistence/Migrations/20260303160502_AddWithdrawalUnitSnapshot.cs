using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalUnitSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_try",
                table: "withdrawal_requests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unit_code",
                table: "withdrawal_requests",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "TRY");

            migrationBuilder.AddColumn<string>(
                name: "unit_label",
                table: "withdrawal_requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "TL");

            migrationBuilder.AddColumn<decimal>(
                name: "unit_try_multiplier",
                table: "withdrawal_requests",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.Sql(
                "UPDATE withdrawal_requests SET amount_try = amount WHERE amount_try = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_try",
                table: "withdrawal_requests");

            migrationBuilder.DropColumn(
                name: "unit_code",
                table: "withdrawal_requests");

            migrationBuilder.DropColumn(
                name: "unit_label",
                table: "withdrawal_requests");

            migrationBuilder.DropColumn(
                name: "unit_try_multiplier",
                table: "withdrawal_requests");
        }
    }
}
