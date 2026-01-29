using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionSystemAndAuditUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "MosqueImages",
                newName: "IsMain");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "MosqueImages",
                newName: "OriginalFileName");

            migrationBuilder.RenameColumn(
                name: "Caption",
                table: "MosqueImages",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "MosqueImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "MosqueImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "MosqueImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "MosqueImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PermissionLevel",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OfficeImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaqfOfficeId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeImages_WaqfOffices_WaqfOfficeId",
                        column: x => x.WaqfOfficeId,
                        principalTable: "WaqfOffices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaqfPropertyId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyImages_WaqfProperties_WaqfPropertyId",
                        column: x => x.WaqfPropertyId,
                        principalTable: "WaqfProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProvinceId",
                table: "AspNetUsers",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeImages_WaqfOfficeId",
                table: "OfficeImages",
                column: "WaqfOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_WaqfPropertyId",
                table: "PropertyImages",
                column: "WaqfPropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Provinces_ProvinceId",
                table: "AspNetUsers",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Provinces_ProvinceId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OfficeImages");

            migrationBuilder.DropTable(
                name: "PropertyImages");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProvinceId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "MosqueImages");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "MosqueImages");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "MosqueImages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "MosqueImages");

            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PermissionLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "OriginalFileName",
                table: "MosqueImages",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "IsMain",
                table: "MosqueImages",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "MosqueImages",
                newName: "Caption");
        }
    }
}
