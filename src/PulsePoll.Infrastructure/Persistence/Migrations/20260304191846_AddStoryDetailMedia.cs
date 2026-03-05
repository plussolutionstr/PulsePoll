using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryDetailMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "story_image_url",
                table: "stories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "story_media_asset_id",
                table: "stories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_stories_story_media_asset_id",
                table: "stories",
                column: "story_media_asset_id");

            migrationBuilder.AddForeignKey(
                name: "fk_stories_story_media_assets",
                table: "stories",
                column: "story_media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stories_story_media_assets",
                table: "stories");

            migrationBuilder.DropIndex(
                name: "idx_stories_story_media_asset_id",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "story_image_url",
                table: "stories");

            migrationBuilder.DropColumn(
                name: "story_media_asset_id",
                table: "stories");
        }
    }
}
