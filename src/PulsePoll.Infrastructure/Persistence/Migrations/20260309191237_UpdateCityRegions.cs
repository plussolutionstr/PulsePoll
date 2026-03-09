using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCityRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 81 ilin Region, Nuts1Code ve Nuts1Region bilgilerini güncelle
            migrationBuilder.Sql("""
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR1', nuts1region = 'İstanbul' WHERE id = 1;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR2', nuts1region = 'Batı Marmara' WHERE id = 2;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 3;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 4;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 5;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR5', nuts1region = 'Batı Anadolu' WHERE id = 6;
                UPDATE cities SET region = 'Akdeniz Bölgesi', nuts1code = 'TR6', nuts1region = 'Akdeniz' WHERE id = 7;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 8;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 9;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR2', nuts1region = 'Batı Marmara' WHERE id = 10;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR2', nuts1region = 'Batı Marmara' WHERE id = 11;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 12;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 13;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR4', nuts1region = 'Doğu Marmara' WHERE id = 14;
                UPDATE cities SET region = 'Akdeniz Bölgesi', nuts1code = 'TR6', nuts1region = 'Akdeniz' WHERE id = 15;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR4', nuts1region = 'Doğu Marmara' WHERE id = 16;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR2', nuts1region = 'Batı Marmara' WHERE id = 17;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 18;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 19;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 20;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 21;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR2', nuts1region = 'Batı Marmara' WHERE id = 22;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 23;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 24;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 25;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR5', nuts1region = 'Batı Anadolu' WHERE id = 26;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 27;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR9', nuts1region = 'Doğu Karadeniz' WHERE id = 28;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 29;
                UPDATE cities SET region = 'Akdeniz Bölgesi', nuts1code = 'TR6', nuts1region = 'Akdeniz' WHERE id = 30;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 31;
                UPDATE cities SET region = 'Akdeniz Bölgesi', nuts1code = 'TR6', nuts1region = 'Akdeniz' WHERE id = 32;
                UPDATE cities SET region = 'Akdeniz Bölgesi', nuts1code = 'TR6', nuts1region = 'Akdeniz' WHERE id = 33;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR1', nuts1region = 'İstanbul' WHERE id = 34;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 35;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 36;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 37;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 38;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 39;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR4', nuts1region = 'Doğu Marmara' WHERE id = 40;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR4', nuts1region = 'Doğu Marmara' WHERE id = 41;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR5', nuts1region = 'Batı Anadolu' WHERE id = 42;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 43;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 44;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 45;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 46;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 47;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 48;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 49;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 50;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 51;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR9', nuts1region = 'Doğu Karadeniz' WHERE id = 52;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR9', nuts1region = 'Doğu Karadeniz' WHERE id = 53;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR4', nuts1region = 'Doğu Marmara' WHERE id = 54;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 55;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 56;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 57;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 58;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR2', nuts1region = 'Batı Marmara' WHERE id = 59;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 60;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR9', nuts1region = 'Doğu Karadeniz' WHERE id = 61;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 62;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 63;
                UPDATE cities SET region = 'Ege Bölgesi', nuts1code = 'TR3', nuts1region = 'Ege' WHERE id = 64;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRB', nuts1region = 'Ortadoğu Anadolu' WHERE id = 65;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 66;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 67;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 68;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 69;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR7', nuts1region = 'Orta Anadolu' WHERE id = 70;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 71;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 72;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 73;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 74;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 75;
                UPDATE cities SET region = 'Doğu Anadolu Bölgesi', nuts1code = 'TRA', nuts1region = 'Kuzeydoğu Anadolu' WHERE id = 76;
                UPDATE cities SET region = 'Marmara Bölgesi', nuts1code = 'TR4', nuts1region = 'Doğu Marmara' WHERE id = 77;
                UPDATE cities SET region = 'İç Anadolu Bölgesi', nuts1code = 'TR5', nuts1region = 'Batı Anadolu' WHERE id = 78;
                UPDATE cities SET region = 'Güneydoğu Anadolu Bölgesi', nuts1code = 'TRC', nuts1region = 'Güneydoğu Anadolu' WHERE id = 79;
                UPDATE cities SET region = 'Akdeniz Bölgesi', nuts1code = 'TR6', nuts1region = 'Akdeniz' WHERE id = 80;
                UPDATE cities SET region = 'Karadeniz Bölgesi', nuts1code = 'TR8', nuts1region = 'Batı Karadeniz' WHERE id = 81;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE cities SET region = NULL, nuts1code = NULL, nuts1region = NULL;
            """);
        }
    }
}
