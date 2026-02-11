using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehicleBrands.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarcasAutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryOfOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FoundedYear = table.Column<int>(type: "integer", nullable: false),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsLuxury = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Headquarters = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasAutos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MarcasAutos",
                columns: new[] { "Id", "CountryOfOrigin", "FoundedYear", "Headquarters", "Name", "Website" },
                values: new object[,]
                {
                    { 1, "Japan", 1937, "Toyota City, Aichi", "Toyota", "https://www.toyota.com" },
                    { 2, "United States", 1903, "Dearborn, Michigan", "Ford", "https://www.ford.com" }
                });

            migrationBuilder.InsertData(
                table: "MarcasAutos",
                columns: new[] { "Id", "CountryOfOrigin", "FoundedYear", "Headquarters", "IsLuxury", "Name", "Website" },
                values: new object[,]
                {
                    { 3, "Germany", 1916, "Munich, Bavaria", true, "BMW", "https://www.bmw.com" },
                    { 4, "Italy", 1947, "Maranello, Modena", true, "Ferrari", "https://www.ferrari.com" }
                });

            migrationBuilder.InsertData(
                table: "MarcasAutos",
                columns: new[] { "Id", "CountryOfOrigin", "FoundedYear", "Headquarters", "Name", "Website" },
                values: new object[] { 5, "South Korea", 1967, "Seoul", "Hyundai", "https://www.hyundai.com" });

            migrationBuilder.CreateIndex(
                name: "IX_MarcasAutos_Name",
                table: "MarcasAutos",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarcasAutos");
        }
    }
}
