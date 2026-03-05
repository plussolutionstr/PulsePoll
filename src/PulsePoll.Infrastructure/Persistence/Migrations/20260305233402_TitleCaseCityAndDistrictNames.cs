using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TitleCaseCityAndDistrictNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE cities SET name = INITCAP(LOWER(name));");
            migrationBuilder.Sql("UPDATE districts SET name = INITCAP(LOWER(name));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE cities SET name = UPPER(name);");
            migrationBuilder.Sql("UPDATE districts SET name = UPPER(name);");
        }
    }
}
