using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalAffiliates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "external_affiliate_id",
                table: "subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "external_affiliates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    affiliate_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    commission_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    trigger_type = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_earned = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_affiliates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "affiliate_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_affiliate_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    subject_id = table.Column<int>(type: "integer", nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_affiliate_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_affiliate_transactions_external_affiliates",
                        column: x => x.external_affiliate_id,
                        principalTable: "external_affiliates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_affiliate_transactions_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_subjects_external_affiliate_id",
                table: "subjects",
                column: "external_affiliate_id");

            migrationBuilder.CreateIndex(
                name: "idx_affiliate_transactions_reference_id",
                table: "affiliate_transactions",
                column: "reference_id",
                unique: true,
                filter: "reference_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_affiliate_transactions_external_affiliate_id",
                table: "affiliate_transactions",
                column: "external_affiliate_id");

            migrationBuilder.CreateIndex(
                name: "ix_affiliate_transactions_subject_id",
                table: "affiliate_transactions",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "idx_external_affiliates_affiliate_code",
                table: "external_affiliates",
                column: "affiliate_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_external_affiliates_email",
                table: "external_affiliates",
                column: "email");

            migrationBuilder.AddForeignKey(
                name: "fk_subjects_external_affiliates",
                table: "subjects",
                column: "external_affiliate_id",
                principalTable: "external_affiliates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_subjects_external_affiliates",
                table: "subjects");

            migrationBuilder.DropTable(
                name: "affiliate_transactions");

            migrationBuilder.DropTable(
                name: "external_affiliates");

            migrationBuilder.DropIndex(
                name: "ix_subjects_external_affiliate_id",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "external_affiliate_id",
                table: "subjects");
        }
    }
}
