using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCategoriesAndMapIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceFacilities_MapIcon_MapIconId",
                table: "ServiceFacilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapIcon",
                table: "MapIcon");

            migrationBuilder.RenameTable(
                name: "MapIcon",
                newName: "MapIcons");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "MapIcons",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "MapIcons",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapIcons",
                table: "MapIcons",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultIconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultIconColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MapIconId = table.Column<int>(type: "int", nullable: true),
                    CustomIconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomIconColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceTypes_MapIcons_MapIconId",
                        column: x => x.MapIconId,
                        principalTable: "MapIcons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceTypes_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapIcons_Category_NameAr",
                table: "MapIcons",
                columns: new[] { "Category", "NameAr" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_NameAr",
                table: "ServiceCategories",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_MapIconId",
                table: "ServiceTypes",
                column: "MapIconId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_ServiceCategoryId_NameAr",
                table: "ServiceTypes",
                columns: new[] { "ServiceCategoryId", "NameAr" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceFacilities_MapIcons_MapIconId",
                table: "ServiceFacilities",
                column: "MapIconId",
                principalTable: "MapIcons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceFacilities_MapIcons_MapIconId",
                table: "ServiceFacilities");

            migrationBuilder.DropTable(
                name: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapIcons",
                table: "MapIcons");

            migrationBuilder.DropIndex(
                name: "IX_MapIcons_Category_NameAr",
                table: "MapIcons");

            migrationBuilder.RenameTable(
                name: "MapIcons",
                newName: "MapIcon");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "MapIcon",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "MapIcon",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapIcon",
                table: "MapIcon",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceFacilities_MapIcon_MapIconId",
                table: "ServiceFacilities",
                column: "MapIconId",
                principalTable: "MapIcon",
                principalColumn: "Id");
        }
    }
}
