using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardUnitConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reward_unit_configs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    unit_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    unit_label = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    try_multiplier = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reward_unit_configs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reward_unit_configs");
        }
    }
}
