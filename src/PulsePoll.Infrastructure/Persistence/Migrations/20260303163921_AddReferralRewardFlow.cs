using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralRewardFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_referrals_referred",
                table: "referrals");

            migrationBuilder.AlterColumn<decimal>(
                name: "commission_earned",
                table: "referrals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "commission_amount_try",
                table: "referrals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "commission_granted_at",
                table: "referrals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "commission_unit_code",
                table: "referrals",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "commission_unit_label",
                table: "referrals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "commission_unit_try_multiplier",
                table: "referrals",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "referral_reward_configs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    reward_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_referral_reward_configs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "uq_referrals_referred_subject",
                table: "referrals",
                column: "referred_subject_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_reward_configs");

            migrationBuilder.DropIndex(
                name: "uq_referrals_referred_subject",
                table: "referrals");

            migrationBuilder.DropColumn(
                name: "commission_amount_try",
                table: "referrals");

            migrationBuilder.DropColumn(
                name: "commission_granted_at",
                table: "referrals");

            migrationBuilder.DropColumn(
                name: "commission_unit_code",
                table: "referrals");

            migrationBuilder.DropColumn(
                name: "commission_unit_label",
                table: "referrals");

            migrationBuilder.DropColumn(
                name: "commission_unit_try_multiplier",
                table: "referrals");

            migrationBuilder.AlterColumn<decimal>(
                name: "commission_earned",
                table: "referrals",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_referrals_referred",
                table: "referrals",
                column: "referred_subject_id");
        }
    }
}
