using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralRewardTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "active_days_threshold",
                table: "referral_reward_configs",
                type: "integer",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.AddColumn<int>(
                name: "trigger_type",
                table: "referral_reward_configs",
                type: "integer",
                nullable: false,
                defaultValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active_days_threshold",
                table: "referral_reward_configs");

            migrationBuilder.DropColumn(
                name: "trigger_type",
                table: "referral_reward_configs");
        }
    }
}
