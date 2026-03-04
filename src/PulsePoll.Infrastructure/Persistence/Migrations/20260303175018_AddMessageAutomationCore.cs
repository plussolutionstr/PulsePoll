using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageAutomationCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "message_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    channel_type = table.Column<int>(type: "integer", nullable: false),
                    sms_text = table.Column<string>(type: "character varying(1600)", maxLength: 1600, nullable: true),
                    push_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    push_body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "message_campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    trigger_type = table.Column<int>(type: "integer", nullable: false),
                    trigger_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    target_gender = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_campaigns", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_campaigns_message_templates",
                        column: x => x.template_id,
                        principalTable: "message_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "message_dispatch_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    campaign_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    occurrence_date = table.Column<DateOnly>(type: "date", nullable: false),
                    channel_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_dispatch_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_dispatch_logs_message_campaigns",
                        column: x => x.campaign_id,
                        principalTable: "message_campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_message_dispatch_logs_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_message_campaigns_trigger",
                table: "message_campaigns",
                columns: new[] { "trigger_type", "trigger_key" });

            migrationBuilder.CreateIndex(
                name: "ix_message_campaigns_template_id",
                table: "message_campaigns",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_dispatch_logs_subject_id",
                table: "message_dispatch_logs",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_message_dispatch_logs_campaign_subject_date_channel",
                table: "message_dispatch_logs",
                columns: new[] { "campaign_id", "subject_id", "occurrence_date", "channel_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_dispatch_logs");

            migrationBuilder.DropTable(
                name: "message_campaigns");

            migrationBuilder.DropTable(
                name: "message_templates");
        }
    }
}
