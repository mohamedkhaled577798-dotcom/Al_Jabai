using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWaqifFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ======== Mosques ========
            migrationBuilder.AddColumn<string>(
                name: "WaqifName",
                table: "Mosques",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaqfDocumentDate",
                table: "Mosques",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfDocumentNumber",
                table: "Mosques",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfConditionText",
                table: "Mosques",
                type: "nvarchar(max)",
                nullable: true);

            // ======== WaqfProperties ========
            migrationBuilder.AddColumn<string>(
                name: "WaqifName",
                table: "WaqfProperties",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaqfDocumentDate",
                table: "WaqfProperties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfDocumentNumber",
                table: "WaqfProperties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfConditionText",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            // ======== WaqfLands ========
            migrationBuilder.AddColumn<string>(
                name: "WaqifName",
                table: "WaqfLands",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaqfDocumentDate",
                table: "WaqfLands",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfDocumentNumber",
                table: "WaqfLands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfConditionText",
                table: "WaqfLands",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WaqifName",          table: "Mosques");
            migrationBuilder.DropColumn(name: "WaqfDocumentDate",   table: "Mosques");
            migrationBuilder.DropColumn(name: "WaqfDocumentNumber", table: "Mosques");
            migrationBuilder.DropColumn(name: "WaqfConditionText",  table: "Mosques");

            migrationBuilder.DropColumn(name: "WaqifName",          table: "WaqfProperties");
            migrationBuilder.DropColumn(name: "WaqfDocumentDate",   table: "WaqfProperties");
            migrationBuilder.DropColumn(name: "WaqfDocumentNumber", table: "WaqfProperties");
            migrationBuilder.DropColumn(name: "WaqfConditionText",  table: "WaqfProperties");

            migrationBuilder.DropColumn(name: "WaqifName",          table: "WaqfLands");
            migrationBuilder.DropColumn(name: "WaqfDocumentDate",   table: "WaqfLands");
            migrationBuilder.DropColumn(name: "WaqfDocumentNumber", table: "WaqfLands");
            migrationBuilder.DropColumn(name: "WaqfConditionText",  table: "WaqfLands");
        }
    }
}
