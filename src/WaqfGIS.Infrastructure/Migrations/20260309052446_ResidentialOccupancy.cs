using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResidentialOccupancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResidentialOccupancy",
                table: "WaqfProperties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OccupancyNotes",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupantMinistry",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupantNationalId",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupantPhone",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccupantStartDate",
                table: "WaqfProperties",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResidentialOccupancy",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "OccupancyNotes",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "OccupantMinistry",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "OccupantNationalId",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "OccupantPhone",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "OccupantStartDate",
                table: "WaqfProperties");
        }
    }
}
