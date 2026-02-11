using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehicleBrands.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MarcasAutos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MarcasAutos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MarcasAutos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MarcasAutos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MarcasAutos",
                keyColumn: "Id",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MarcasAutos",
                columns: new[] { "Id", "CountryOfOrigin", "FoundedYear", "Headquarters", "IsActive", "Name", "Website" },
                values: new object[,]
                {
                    { 1, "Japan", 1937, "Toyota City, Aichi", true, "Toyota", "https://www.toyota.com" },
                    { 2, "United States", 1903, "Dearborn, Michigan", true, "Ford", "https://www.ford.com" }
                });

            migrationBuilder.InsertData(
                table: "MarcasAutos",
                columns: new[] { "Id", "CountryOfOrigin", "FoundedYear", "Headquarters", "IsActive", "IsLuxury", "Name", "Website" },
                values: new object[,]
                {
                    { 3, "Germany", 1916, "Munich, Bavaria", true, true, "BMW", "https://www.bmw.com" },
                    { 4, "Italy", 1947, "Maranello, Modena", true, true, "Ferrari", "https://www.ferrari.com" }
                });

            migrationBuilder.InsertData(
                table: "MarcasAutos",
                columns: new[] { "Id", "CountryOfOrigin", "FoundedYear", "Headquarters", "IsActive", "Name", "Website" },
                values: new object[] { 5, "South Korea", 1967, "Seoul", true, "Hyundai", "https://www.hyundai.com" });
        }
    }
}
