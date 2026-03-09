using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EncroachmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EncroachmentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    EncroachmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncroachmentAreaSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EncroachwrName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncroachwrPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncroachwrNationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geometry", nullable: true),
                    LocationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscoveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsReportedToAuthorities = table.Column<bool>(type: "bit", nullable: false),
                    ReportReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorityReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LegalDisputeId = table.Column<int>(type: "int", nullable: true),
                    HasLegalCase = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncroachmentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncroachmentRecords_LegalDisputes_LegalDisputeId",
                        column: x => x.LegalDisputeId,
                        principalTable: "LegalDisputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EncroachmentRecords_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EncroachmentPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncroachmentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncroachmentPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncroachmentPhotos_EncroachmentRecords_EncroachmentId",
                        column: x => x.EncroachmentId,
                        principalTable: "EncroachmentRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EncroachmentPhotos_EncroachmentId",
                table: "EncroachmentPhotos",
                column: "EncroachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EncroachmentRecords_DiscoveryDate",
                table: "EncroachmentRecords",
                column: "DiscoveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_EncroachmentRecords_EntityType_EntityId",
                table: "EncroachmentRecords",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncroachmentRecords_LegalDisputeId",
                table: "EncroachmentRecords",
                column: "LegalDisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_EncroachmentRecords_ProvinceId",
                table: "EncroachmentRecords",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_EncroachmentRecords_Status",
                table: "EncroachmentRecords",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncroachmentPhotos");

            migrationBuilder.DropTable(
                name: "EncroachmentRecords");
        }
    }
}
