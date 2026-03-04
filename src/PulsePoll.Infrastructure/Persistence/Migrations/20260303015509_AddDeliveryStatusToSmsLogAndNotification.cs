using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryStatusToSmsLogAndNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sms_logs_created_at",
                table: "sms_logs");

            migrationBuilder.RenameIndex(
                name: "idx_sms_logs_phone_number",
                table: "sms_logs",
                newName: "idx_sms_logs_phone");

            migrationBuilder.AddColumn<int>(
                name: "delivery_status",
                table: "sms_logs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "sms_logs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "delivery_status",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_sms_logs_status_created",
                table: "sms_logs",
                columns: new[] { "delivery_status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_notifications_status_created",
                table: "notifications",
                columns: new[] { "delivery_status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sms_logs_status_created",
                table: "sms_logs");

            migrationBuilder.DropIndex(
                name: "idx_notifications_status_created",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "delivery_status",
                table: "sms_logs");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "sms_logs");

            migrationBuilder.DropColumn(
                name: "delivery_status",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "notifications");

            migrationBuilder.RenameIndex(
                name: "idx_sms_logs_phone",
                table: "sms_logs",
                newName: "idx_sms_logs_phone_number");

            migrationBuilder.CreateIndex(
                name: "idx_sms_logs_created_at",
                table: "sms_logs",
                column: "created_at");
        }
    }
}
