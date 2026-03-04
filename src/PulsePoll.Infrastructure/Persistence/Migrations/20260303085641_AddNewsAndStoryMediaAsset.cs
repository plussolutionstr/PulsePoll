using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsAndStoryMediaAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "media_asset_id",
                table: "stories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "news",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    media_asset_id = table.Column<int>(type: "integer", nullable: true),
                    link_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news", x => x.id);
                    table.ForeignKey(
                        name: "fk_news_media_assets",
                        column: x => x.media_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_stories_media_asset_id",
                table: "stories",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "idx_news_is_active_starts_at_ends_at",
                table: "news",
                columns: new[] { "is_active", "starts_at", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "idx_news_media_asset_id",
                table: "news",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "idx_news_order",
                table: "news",
                column: "order");

            migrationBuilder.AddForeignKey(
                name: "fk_stories_media_assets",
                table: "stories",
                column: "media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stories_media_assets",
                table: "stories");

            migrationBuilder.DropTable(
                name: "news");

            migrationBuilder.DropIndex(
                name: "idx_stories_media_asset_id",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "media_asset_id",
                table: "stories");
        }
    }
}
