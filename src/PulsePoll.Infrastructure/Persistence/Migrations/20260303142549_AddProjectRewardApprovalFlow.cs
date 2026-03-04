using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectRewardApprovalFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "earned_amount",
                table: "project_assignments",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reward_processed_at",
                table: "project_assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reward_processed_by",
                table: "project_assignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reward_rejection_reason",
                table: "project_assignments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reward_status",
                table: "project_assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reward_processed_at",
                table: "project_assignments");

            migrationBuilder.DropColumn(
                name: "reward_processed_by",
                table: "project_assignments");

            migrationBuilder.DropColumn(
                name: "reward_rejection_reason",
                table: "project_assignments");

            migrationBuilder.DropColumn(
                name: "reward_status",
                table: "project_assignments");

            migrationBuilder.AlterColumn<decimal>(
                name: "earned_amount",
                table: "project_assignments",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);
        }
    }
}
