using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankMediaAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "logo_url",
                table: "banks");

            migrationBuilder.AddColumn<int>(
                name: "logo_media_asset_id",
                table: "banks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "thumbnail_media_asset_id",
                table: "banks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_banks_logo_media_asset_id",
                table: "banks",
                column: "logo_media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_banks_thumbnail_media_asset_id",
                table: "banks",
                column: "thumbnail_media_asset_id");

            migrationBuilder.AddForeignKey(
                name: "fk_banks_logo_media_asset",
                table: "banks",
                column: "logo_media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_banks_thumbnail_media_asset",
                table: "banks",
                column: "thumbnail_media_asset_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_banks_logo_media_asset",
                table: "banks");

            migrationBuilder.DropForeignKey(
                name: "fk_banks_thumbnail_media_asset",
                table: "banks");

            migrationBuilder.DropIndex(
                name: "ix_banks_logo_media_asset_id",
                table: "banks");

            migrationBuilder.DropIndex(
                name: "ix_banks_thumbnail_media_asset_id",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "logo_media_asset_id",
                table: "banks");

            migrationBuilder.DropColumn(
                name: "thumbnail_media_asset_id",
                table: "banks");

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                table: "banks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
