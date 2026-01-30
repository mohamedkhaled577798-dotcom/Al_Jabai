using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGisPhase2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeometryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldGeometryWkt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewGeometryWkt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldAreaSqm = table.Column<double>(type: "float", nullable: true),
                    NewAreaSqm = table.Column<double>(type: "float", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeometryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GisLayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FillColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FillOpacity = table.Column<double>(type: "float", nullable: false),
                    StrokeColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrokeWidth = table.Column<double>(type: "float", nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    MinZoom = table.Column<int>(type: "int", nullable: true),
                    MaxZoom = table.Column<int>(type: "int", nullable: true),
                    SourceTable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeometryColumn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GisLayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MosqueBoundaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MosqueId = table.Column<int>(type: "int", nullable: false),
                    Boundary = table.Column<Polygon>(type: "geometry", nullable: true),
                    CalculatedAreaSqm = table.Column<double>(type: "float", nullable: true),
                    PerimeterMeters = table.Column<double>(type: "float", nullable: true),
                    BoundaryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MosqueBoundaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MosqueBoundaries_Mosques_MosqueId",
                        column: x => x.MosqueId,
                        principalTable: "Mosques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NearbyProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geometry", nullable: true),
                    Boundary = table.Column<Geometry>(type: "geometry", nullable: true),
                    CalculatedAreaSqm = table.Column<double>(type: "float", nullable: true),
                    ProjectType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeveloperName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProjectValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NearbyProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NearbyProjects_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NearbyProjects_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Geometry = table.Column<LineString>(type: "geometry", nullable: true),
                    LengthMeters = table.Column<double>(type: "float", nullable: true),
                    RoadType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WidthMeters = table.Column<double>(type: "float", nullable: true),
                    LanesCount = table.Column<int>(type: "int", nullable: true),
                    SurfaceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roads_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roads_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WaqfLands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WaqfOfficeId = table.Column<int>(type: "int", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CenterPoint = table.Column<Point>(type: "geometry", nullable: true),
                    Boundary = table.Column<Geometry>(type: "geometry", nullable: true),
                    CalculatedAreaSqm = table.Column<double>(type: "float", nullable: true),
                    PerimeterMeters = table.Column<double>(type: "float", nullable: true),
                    LegalAreaSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AreaDonum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LandType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LandUse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZoningCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeedNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnershipStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaqfLands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaqfLands_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WaqfLands_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaqfLands_WaqfOffices_WaqfOfficeId",
                        column: x => x.WaqfOfficeId,
                        principalTable: "WaqfOffices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeometryAuditLogs_EntityType_EntityId",
                table: "GeometryAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_GeometryAuditLogs_Timestamp",
                table: "GeometryAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_GisLayers_Code",
                table: "GisLayers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MosqueBoundaries_MosqueId",
                table: "MosqueBoundaries",
                column: "MosqueId");

            migrationBuilder.CreateIndex(
                name: "IX_NearbyProjects_Code",
                table: "NearbyProjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NearbyProjects_DistrictId",
                table: "NearbyProjects",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_NearbyProjects_ProvinceId",
                table: "NearbyProjects",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Roads_Code",
                table: "Roads",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roads_DistrictId",
                table: "Roads",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Roads_ProvinceId",
                table: "Roads",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_WaqfLands_Code",
                table: "WaqfLands",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaqfLands_DistrictId",
                table: "WaqfLands",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_WaqfLands_ProvinceId",
                table: "WaqfLands",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_WaqfLands_WaqfOfficeId",
                table: "WaqfLands",
                column: "WaqfOfficeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeometryAuditLogs");

            migrationBuilder.DropTable(
                name: "GisLayers");

            migrationBuilder.DropTable(
                name: "MosqueBoundaries");

            migrationBuilder.DropTable(
                name: "NearbyProjects");

            migrationBuilder.DropTable(
                name: "Roads");

            migrationBuilder.DropTable(
                name: "WaqfLands");
        }
    }
}
