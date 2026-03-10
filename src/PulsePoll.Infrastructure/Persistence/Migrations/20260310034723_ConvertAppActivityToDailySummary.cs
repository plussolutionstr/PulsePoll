using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertAppActivityToDailySummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Mevcut verileri geçici tabloya özetle
            migrationBuilder.Sql("""
                CREATE TEMP TABLE _activity_summary AS
                SELECT
                    subject_id,
                    (occurred_at::date)::date AS activity_date,
                    MIN(occurred_at) AS first_open_at,
                    MAX(occurred_at) AS last_seen_at,
                    COUNT(*) FILTER (WHERE type = 1) AS open_count,
                    COUNT(*) FILTER (WHERE type = 2) * 5 AS total_minutes,
                    (array_agg(platform ORDER BY occurred_at DESC))[1] AS platform,
                    (array_agg(app_version ORDER BY occurred_at DESC))[1] AS app_version,
                    (array_agg(device_id_hash ORDER BY occurred_at DESC))[1] AS device_id_hash,
                    MIN(created_by) AS created_by,
                    MIN(created_at) AS created_at
                FROM subject_app_activities
                GROUP BY subject_id, occurred_at::date;
                """);

            // 2. Mevcut verileri temizle
            migrationBuilder.Sql("TRUNCATE TABLE subject_app_activities RESTART IDENTITY;");

            // 3. Eski index'i kaldır
            migrationBuilder.DropIndex(
                name: "idx_subject_app_activities_subject_id_occurred_at",
                table: "subject_app_activities");

            // 4. Kolon değişiklikleri
            migrationBuilder.RenameColumn(
                name: "type",
                table: "subject_app_activities",
                newName: "total_minutes");

            migrationBuilder.RenameColumn(
                name: "occurred_at",
                table: "subject_app_activities",
                newName: "last_seen_at");

            migrationBuilder.AddColumn<DateOnly>(
                name: "activity_date",
                table: "subject_app_activities",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "first_open_at",
                table: "subject_app_activities",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "open_count",
                table: "subject_app_activities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 5. Unique index oluştur
            migrationBuilder.CreateIndex(
                name: "idx_subject_app_activities_subject_date",
                table: "subject_app_activities",
                columns: new[] { "subject_id", "activity_date" },
                unique: true);

            // 6. Özetlenmiş veriyi geri yükle
            migrationBuilder.Sql("""
                INSERT INTO subject_app_activities
                    (subject_id, activity_date, first_open_at, last_seen_at, open_count,
                     total_minutes, platform, app_version, device_id_hash,
                     created_by, created_at)
                SELECT
                    subject_id, activity_date, first_open_at, last_seen_at, open_count,
                    total_minutes, platform, app_version, device_id_hash,
                    created_by, created_at
                FROM _activity_summary;
                """);

            migrationBuilder.Sql("DROP TABLE IF EXISTS _activity_summary;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_subject_app_activities_subject_date",
                table: "subject_app_activities");

            migrationBuilder.DropColumn(
                name: "activity_date",
                table: "subject_app_activities");

            migrationBuilder.DropColumn(
                name: "first_open_at",
                table: "subject_app_activities");

            migrationBuilder.DropColumn(
                name: "open_count",
                table: "subject_app_activities");

            migrationBuilder.RenameColumn(
                name: "total_minutes",
                table: "subject_app_activities",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "last_seen_at",
                table: "subject_app_activities",
                newName: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "idx_subject_app_activities_subject_id_occurred_at",
                table: "subject_app_activities",
                columns: new[] { "subject_id", "occurred_at" });
        }
    }
}
