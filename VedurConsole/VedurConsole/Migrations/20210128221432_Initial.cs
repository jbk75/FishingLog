using Microsoft.EntityFrameworkCore.Migrations;

namespace VedurConsole.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Weather",
                columns: table => new
                {
                    WeatherID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Breidd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Daggarmark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Haed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hiti = table.Column<double>(type: "float", nullable: false),
                    Lengd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loftthrystingur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nafn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nr = table.Column<int>(type: "int", nullable: false),
                    Nr_Vedurstofa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PntX = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PntY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Raki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sjavarhaed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vindatt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VindattAsc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VindattStDev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vindhradi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vindhvida = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weather", x => x.WeatherID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Weather");
        }
    }
}
