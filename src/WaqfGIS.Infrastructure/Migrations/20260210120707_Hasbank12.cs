using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Hasbank12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomIconClass",
                table: "ServiceFacilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomIconColor",
                table: "ServiceFacilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MapIconId",
                table: "ServiceFacilities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MapIcon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconShape = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconSize = table.Column<int>(type: "int", nullable: false),
                    UsedFor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemIcon = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CustomSvg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapIcon", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFacilities_MapIconId",
                table: "ServiceFacilities",
                column: "MapIconId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceFacilities_MapIcon_MapIconId",
                table: "ServiceFacilities",
                column: "MapIconId",
                principalTable: "MapIcon",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceFacilities_MapIcon_MapIconId",
                table: "ServiceFacilities");

            migrationBuilder.DropTable(
                name: "MapIcon");

            migrationBuilder.DropIndex(
                name: "IX_ServiceFacilities_MapIconId",
                table: "ServiceFacilities");

            migrationBuilder.DropColumn(
                name: "CustomIconClass",
                table: "ServiceFacilities");

            migrationBuilder.DropColumn(
                name: "CustomIconColor",
                table: "ServiceFacilities");

            migrationBuilder.DropColumn(
                name: "MapIconId",
                table: "ServiceFacilities");
        }
    }
}
