using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_batches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    item_count = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_batch_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_batch_id = table.Column<int>(type: "integer", nullable: false),
                    withdrawal_request_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_batch_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_batch_items_batch",
                        column: x => x.payment_batch_id,
                        principalTable: "payment_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_batch_items_withdrawal",
                        column: x => x.withdrawal_request_id,
                        principalTable: "withdrawal_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_batch_items_payment_batch_id",
                table: "payment_batch_items",
                column: "payment_batch_id");

            migrationBuilder.CreateIndex(
                name: "uq_payment_batch_items_withdrawal_request",
                table: "payment_batch_items",
                column: "withdrawal_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payment_batches_status",
                table: "payment_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "uq_payment_batches_batch_number",
                table: "payment_batches",
                column: "batch_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_payment_settings_key",
                table: "payment_settings",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_batch_items");

            migrationBuilder.DropTable(
                name: "payment_settings");

            migrationBuilder.DropTable(
                name: "payment_batches");
        }
    }
}
