using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newBorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncroachmentNotes",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasEncroachment",
                table: "WaqfProperties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminReceived",
                table: "WaqfProperties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOccupantRetired",
                table: "WaqfProperties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupantEmployeeName",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfCondition",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaqfNature",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EncroachmentNotes",
                table: "WaqfLands",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasEncroachment",
                table: "WaqfLands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminReceived",
                table: "WaqfLands",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfCondition",
                table: "WaqfLands",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaqfNature",
                table: "WaqfLands",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminReceived",
                table: "Mosques",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Mosques",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsContested",
                table: "Mosques",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsurped",
                table: "Mosques",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WaqfCondition",
                table: "Mosques",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaqfNature",
                table: "Mosques",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MosqueRegistrationFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MosqueId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_MosqueRegistrationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MosqueRegistrationFiles_Mosques_MosqueId",
                        column: x => x.MosqueId,
                        principalTable: "Mosques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyRegistrationFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PropertyRegistrationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyRegistrationFiles_WaqfProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "WaqfProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaqfLandRegistrationFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaqfLandId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WaqfLandRegistrationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaqfLandRegistrationFiles_WaqfLands_WaqfLandId",
                        column: x => x.WaqfLandId,
                        principalTable: "WaqfLands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MosqueRegistrationFiles_MosqueId",
                table: "MosqueRegistrationFiles",
                column: "MosqueId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRegistrationFiles_PropertyId",
                table: "PropertyRegistrationFiles",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_WaqfLandRegistrationFiles_WaqfLandId",
                table: "WaqfLandRegistrationFiles",
                column: "WaqfLandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MosqueRegistrationFiles");

            migrationBuilder.DropTable(
                name: "PropertyRegistrationFiles");

            migrationBuilder.DropTable(
                name: "WaqfLandRegistrationFiles");

            migrationBuilder.DropColumn(
                name: "EncroachmentNotes",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "HasEncroachment",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "IsAdminReceived",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "IsOccupantRetired",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "OccupantEmployeeName",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "WaqfCondition",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "WaqfNature",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "EncroachmentNotes",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "HasEncroachment",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "IsAdminReceived",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "WaqfCondition",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "WaqfNature",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "IsAdminReceived",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "IsContested",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "IsUsurped",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "WaqfCondition",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "WaqfNature",
                table: "Mosques");
        }
    }
}
