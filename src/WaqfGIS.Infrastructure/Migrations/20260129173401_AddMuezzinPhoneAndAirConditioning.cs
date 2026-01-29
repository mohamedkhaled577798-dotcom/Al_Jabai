using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMuezzinPhoneAndAirConditioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAirConditioning",
                table: "Mosques",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MuezzinPhone",
                table: "Mosques",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAirConditioning",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "MuezzinPhone",
                table: "Mosques");
        }
    }
}
