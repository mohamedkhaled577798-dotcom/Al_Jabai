using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaqfSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConnectionStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GisLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GisLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Governorates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GisLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    FullNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubDistricts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GisLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDistricts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubDistricts_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, collation: "Arabic_CI_AS"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    MissionType = table.Column<byte>(type: "tinyint", nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    AssignedTeamId = table.Column<int>(type: "int", nullable: true),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetPropertyCount = table.Column<int>(type: "int", nullable: true),
                    CompletedPropertyCount = table.Column<int>(type: "int", nullable: false),
                    CheckInLatitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    CheckInLongitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    CheckInAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    ProgressPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionMissions_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionMissions_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionMissions_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionMissions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionMissions_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionMissions_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false, collation: "Arabic_CI_AS"),
                    NotificationType = table.Column<byte>(type: "tinyint", nullable: false),
                    ReferenceTable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WqfNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true, collation: "Arabic_CI_AS"),
                    PropertyNameEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PropertyType = table.Column<byte>(type: "tinyint", nullable: false),
                    PropertyCategory = table.Column<byte>(type: "tinyint", nullable: false),
                    WaqfType = table.Column<byte>(type: "tinyint", nullable: true),
                    OwnershipType = table.Column<byte>(type: "tinyint", nullable: false),
                    OwnershipPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DeedNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CadastralNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TabuNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WaqfOriginStory = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    FounderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    FoundationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndowmentPurpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    TotalFloors = table.Column<short>(type: "smallint", nullable: true),
                    BasementFloors = table.Column<short>(type: "smallint", nullable: true),
                    TotalUnits = table.Column<int>(type: "int", nullable: true),
                    TotalAreaSqm = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    BuiltUpAreaSqm = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    LandAreaSqm = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    YearBuilt = table.Column<short>(type: "smallint", nullable: true),
                    LastRenovationYear = table.Column<short>(type: "smallint", nullable: true),
                    ConstructionType = table.Column<byte>(type: "tinyint", nullable: true),
                    StructuralCondition = table.Column<byte>(type: "tinyint", nullable: true),
                    FacadeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    RoofType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastValuationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InsuranceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PropertyStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    ApprovalStage = table.Column<byte>(type: "tinyint", nullable: false),
                    DqsScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    GisFeatureId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GisPolygonId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GisLayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GisPolygon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    GpsAccuracyMeters = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    SatelliteImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastGisSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GisSyncStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    LocalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true),
                    GovernorateId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Properties_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubDistrictId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GisLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neighborhoods_SubDistricts_SubDistrictId",
                        column: x => x.SubDistrictId,
                        principalTable: "SubDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgriculturalDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    TotalAreaDunum = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    CultivatedAreaDunum = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    UncultivatedAreaDunum = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SoilType = table.Column<byte>(type: "tinyint", nullable: true),
                    SoilFertilityRating = table.Column<byte>(type: "tinyint", nullable: true),
                    WaterSourceType = table.Column<byte>(type: "tinyint", nullable: true),
                    WaterRightsDocUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IrrigationMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    WaterAvailabilityMonths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryHarvestType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    SecondaryHarvestType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    SeasonType = table.Column<byte>(type: "tinyint", nullable: true),
                    AverageYieldTonPerDunum = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    FarmerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    FarmerNationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FarmingContractType = table.Column<byte>(type: "tinyint", nullable: true),
                    WaqfShareOfHarvest = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FarmingStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FarmingEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasFarmBuilding = table.Column<bool>(type: "bit", nullable: false),
                    HasStorage = table.Column<bool>(type: "bit", nullable: false),
                    HasWell = table.Column<bool>(type: "bit", nullable: false),
                    HasRoadAccess = table.Column<bool>(type: "bit", nullable: false),
                    AnnualRevenueEstimate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LandValuePerDunum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastInspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastHarvestYear = table.Column<short>(type: "smallint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgriculturalDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgriculturalDetails_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgriculturalDetails_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgriculturalDetails_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GisSyncLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RequestPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GisSyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GisSyncLogs_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GisSyncLogs_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    DocumentCategory = table.Column<byte>(type: "tinyint", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    IssuingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileFormat = table.Column<byte>(type: "tinyint", nullable: false),
                    FileSizeKB = table.Column<int>(type: "int", nullable: true),
                    IsOriginal = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerificationMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    VerifiedById = table.Column<int>(type: "int", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OcrText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcrConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GisAttachedLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FacilityType = table.Column<byte>(type: "tinyint", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFacilities_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyFacilities_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyFacilities_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyFloors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FloorNumber = table.Column<short>(type: "smallint", nullable: false),
                    FloorLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, collation: "Arabic_CI_AS"),
                    FloorUsage = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalAreaSqm = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    UsableAreaSqm = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CeilingHeightCm = table.Column<short>(type: "smallint", nullable: true),
                    StructuralCondition = table.Column<byte>(type: "tinyint", nullable: true),
                    HasBalcony = table.Column<bool>(type: "bit", nullable: false),
                    FloorPlanUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFloors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFloors_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyFloors_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyFloors_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyPartnerships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    PartnerType = table.Column<byte>(type: "tinyint", nullable: false),
                    PartnerNationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PartnerSharePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PartnerBankIBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    RevenueDistribMethod = table.Column<byte>(type: "tinyint", nullable: false),
                    AgreementDocUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AgreementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPartnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyPartnerships_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyPartnerships_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyPartnerships_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyWorkflowHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FromStage = table.Column<byte>(type: "tinyint", nullable: false),
                    ToStage = table.Column<byte>(type: "tinyint", nullable: false),
                    ActionById = table.Column<int>(type: "int", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    DqsAtAction = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyWorkflowHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyWorkflowHistory_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyWorkflowHistory_Users_ActionById",
                        column: x => x.ActionById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyWorkflowHistory_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Streets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GisLayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streets_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FloorId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, collation: "Arabic_CI_AS"),
                    UnitType = table.Column<byte>(type: "tinyint", nullable: false),
                    AreaSqm = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    BedroomCount = table.Column<short>(type: "smallint", nullable: true),
                    BathroomCount = table.Column<short>(type: "smallint", nullable: true),
                    OccupancyStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentContractId = table.Column<int>(type: "int", nullable: true),
                    MarketRentMonthly = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    ElectricMeterNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WaterMeterNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasAC = table.Column<bool>(type: "bit", nullable: false),
                    HasKitchen = table.Column<bool>(type: "bit", nullable: false),
                    Furnished = table.Column<byte>(type: "tinyint", nullable: false),
                    Condition = table.Column<byte>(type: "tinyint", nullable: true),
                    PhotoUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitFloorPlanUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyUnits_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyUnits_PropertyFloors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "PropertyFloors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyUnits_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyUnits_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    StreetId = table.Column<int>(type: "int", nullable: true),
                    BuildingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlotNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BlockNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ZoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NearestLandmark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true, collation: "Arabic_CI_AS"),
                    AlternativeAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    What3Words = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlusCodes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyAddresses_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyAddresses_Streets_StreetId",
                        column: x => x.StreetId,
                        principalTable: "Streets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyAddresses_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyAddresses_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyMeters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    MeterType = table.Column<byte>(type: "tinyint", nullable: false),
                    MeterNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubscriberNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyMeters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyMeters_PropertyUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyMeters_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyMeters_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    PhotoType = table.Column<byte>(type: "tinyint", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSizeKB = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceAccuracy = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true, collation: "Arabic_CI_AS"),
                    UploadedById = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyPhotos_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyPhotos_PropertyUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyPhotos_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyPhotos_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<byte>(type: "tinyint", nullable: false),
                    AreaSqm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    Length = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    WindowsCount = table.Column<short>(type: "smallint", nullable: true),
                    Condition = table.Column<byte>(type: "tinyint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyRooms_PropertyUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyRooms_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyRooms_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "CreatedAt", "GisLayerId", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt" },
                values: new object[] { 1, "IQ", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(376), null, true, false, "العراق", "Iraq", null });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "SYS_ADMIN", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(619), null, true, false, "مدير النظام", "System Administrator", null },
                    { 2, "AUTH_DIRECTOR", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(623), null, true, false, "مدير الهيئة", "Authority Director", null },
                    { 3, "REGIONAL_MGR", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(624), null, true, false, "مدير إقليمي", "Regional Manager", null },
                    { 4, "LEGAL_REVIEWER", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(625), null, true, false, "مراجع قانوني", "Legal Reviewer", null },
                    { 5, "ENGINEER", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(625), null, true, false, "مهندس", "Engineer", null },
                    { 6, "FIELD_SUPERVISOR", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(626), null, true, false, "مشرف ميداني", "Field Supervisor", null },
                    { 7, "FIELD_INSPECTOR", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(627), null, true, false, "باحث ميداني", "Field Inspector", null },
                    { 8, "COLLECTOR", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(628), null, true, false, "جابي", "Collector", null },
                    { 9, "CONTRACTS_MGR", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(629), null, true, false, "مدير العقود", "Contracts Manager", null },
                    { 10, "ANALYST", new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(630), null, true, false, "محلل", "Analyst", null }
                });

            migrationBuilder.InsertData(
                table: "Governorates",
                columns: new[] { "Id", "Code", "CountryId", "CreatedAt", "GisLayerId", "IsActive", "IsDeleted", "NameAr", "NameEn", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "BGW", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(562), null, true, false, "بغداد", "Baghdad", null },
                    { 2, "BSA", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(565), null, true, false, "البصرة", "Basra", null },
                    { 3, "NIN", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(567), null, true, false, "نينوى", "Nineveh", null },
                    { 4, "EBL", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(568), null, true, false, "أربيل", "Erbil", null },
                    { 5, "SLM", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(569), null, true, false, "السليمانية", "Sulaymaniyah", null },
                    { 6, "DHK", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(570), null, true, false, "دهوك", "Duhok", null },
                    { 7, "KRK", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(571), null, true, false, "كركوك", "Kirkuk", null },
                    { 8, "DIY", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(572), null, true, false, "ديالى", "Diyala", null },
                    { 9, "ANB", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(573), null, true, false, "الأنبار", "Anbar", null },
                    { 10, "BAB", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(574), null, true, false, "بابل", "Babylon", null },
                    { 11, "KAR", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(575), null, true, false, "كربلاء", "Karbala", null },
                    { 12, "NAJ", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(575), null, true, false, "النجف", "Najaf", null },
                    { 13, "WAS", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(576), null, true, false, "واسط", "Wasit", null },
                    { 14, "SLD", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(578), null, true, false, "صلاح الدين", "Salahuddin", null },
                    { 15, "DHQ", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(578), null, true, false, "ذي قار", "Dhi Qar", null },
                    { 16, "MYS", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(579), null, true, false, "ميسان", "Maysan", null },
                    { 17, "MTN", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(581), null, true, false, "المثنى", "Muthanna", null },
                    { 18, "QAD", 1, new DateTime(2026, 3, 13, 20, 24, 41, 874, DateTimeKind.Utc).AddTicks(582), null, true, false, "القادسية", "Qadisiyah", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgriculturalDetails_CreatedById",
                table: "AgriculturalDetails",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AgriculturalDetails_PropertyId",
                table: "AgriculturalDetails",
                column: "PropertyId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AgriculturalDetails_UpdatedById",
                table: "AgriculturalDetails",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName_RecordId",
                table: "AuditLogs",
                columns: new[] { "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_GovernorateId",
                table: "Districts",
                column: "GovernorateId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GisSyncLogs_PropertyId",
                table: "GisSyncLogs",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GisSyncLogs_Status",
                table: "GisSyncLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GisSyncLogs_UpdatedById",
                table: "GisSyncLogs",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_Code",
                table: "Governorates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_CountryId",
                table: "Governorates",
                column: "CountryId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_AssignedToId",
                table: "InspectionMissions",
                column: "AssignedToId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_CreatedById",
                table: "InspectionMissions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_DistrictId",
                table: "InspectionMissions",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_GovernorateId",
                table: "InspectionMissions",
                column: "GovernorateId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_MissionNumber",
                table: "InspectionMissions",
                column: "MissionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_ScheduledDate",
                table: "InspectionMissions",
                column: "ScheduledDate",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_Status",
                table: "InspectionMissions",
                column: "Status",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_SupervisorId",
                table: "InspectionMissions",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_UpdatedById",
                table: "InspectionMissions",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_SubDistrictId",
                table: "Neighborhoods",
                column: "SubDistrictId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UpdatedById",
                table: "Notifications",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ApprovalStage",
                table: "Properties",
                column: "ApprovalStage",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CreatedById",
                table: "Properties",
                column: "CreatedById",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_DqsScore",
                table: "Properties",
                column: "DqsScore",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_GisFeatureId",
                table: "Properties",
                column: "GisFeatureId",
                filter: "[GisFeatureId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_GisSyncStatus",
                table: "Properties",
                column: "GisSyncStatus",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_GovernorateId",
                table: "Properties",
                column: "GovernorateId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Latitude_Longitude",
                table: "Properties",
                columns: new[] { "Latitude", "Longitude" },
                filter: "[Latitude] IS NOT NULL AND [Longitude] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_LocalId",
                table: "Properties",
                column: "LocalId",
                filter: "[LocalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnershipType",
                table: "Properties",
                column: "OwnershipType",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PropertyStatus",
                table: "Properties",
                column: "PropertyStatus",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PropertyType",
                table: "Properties",
                column: "PropertyType",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_UpdatedById",
                table: "Properties",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_WqfNumber",
                table: "Properties",
                column: "WqfNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAddresses_CreatedById",
                table: "PropertyAddresses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAddresses_PropertyId",
                table: "PropertyAddresses",
                column: "PropertyId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAddresses_StreetId",
                table: "PropertyAddresses",
                column: "StreetId",
                filter: "[StreetId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAddresses_UpdatedById",
                table: "PropertyAddresses",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_CreatedById",
                table: "PropertyDocuments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_DocumentCategory",
                table: "PropertyDocuments",
                column: "DocumentCategory",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_PropertyId",
                table: "PropertyDocuments",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_UpdatedById",
                table: "PropertyDocuments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_VerifiedById",
                table: "PropertyDocuments",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilities_CreatedById",
                table: "PropertyFacilities",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilities_PropertyId",
                table: "PropertyFacilities",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilities_UpdatedById",
                table: "PropertyFacilities",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFloors_CreatedById",
                table: "PropertyFloors",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFloors_PropertyId",
                table: "PropertyFloors",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFloors_UpdatedById",
                table: "PropertyFloors",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMeters_CreatedById",
                table: "PropertyMeters",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMeters_UnitId",
                table: "PropertyMeters",
                column: "UnitId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMeters_UpdatedById",
                table: "PropertyMeters",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPartnerships_CreatedById",
                table: "PropertyPartnerships",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPartnerships_PropertyId",
                table: "PropertyPartnerships",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPartnerships_UpdatedById",
                table: "PropertyPartnerships",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPhotos_PropertyId",
                table: "PropertyPhotos",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPhotos_UnitId",
                table: "PropertyPhotos",
                column: "UnitId",
                filter: "[UnitId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPhotos_UpdatedById",
                table: "PropertyPhotos",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPhotos_UploadedById",
                table: "PropertyPhotos",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRooms_CreatedById",
                table: "PropertyRooms",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRooms_UnitId",
                table: "PropertyRooms",
                column: "UnitId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRooms_UpdatedById",
                table: "PropertyRooms",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_CreatedById",
                table: "PropertyUnits",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_FloorId",
                table: "PropertyUnits",
                column: "FloorId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_OccupancyStatus",
                table: "PropertyUnits",
                column: "OccupancyStatus",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_PropertyId",
                table: "PropertyUnits",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_UpdatedById",
                table: "PropertyUnits",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyWorkflowHistory_ActionById",
                table: "PropertyWorkflowHistory",
                column: "ActionById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyWorkflowHistory_PropertyId",
                table: "PropertyWorkflowHistory",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyWorkflowHistory_UpdatedById",
                table: "PropertyWorkflowHistory",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streets_NeighborhoodId",
                table: "Streets",
                column: "NeighborhoodId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SubDistricts_DistrictId",
                table: "SubDistricts",
                column: "DistrictId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedById",
                table: "Users",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GovernorateId",
                table: "Users",
                column: "GovernorateId",
                filter: "[GovernorateId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgriculturalDetails");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "GisSyncLogs");

            migrationBuilder.DropTable(
                name: "InspectionMissions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PropertyAddresses");

            migrationBuilder.DropTable(
                name: "PropertyDocuments");

            migrationBuilder.DropTable(
                name: "PropertyFacilities");

            migrationBuilder.DropTable(
                name: "PropertyMeters");

            migrationBuilder.DropTable(
                name: "PropertyPartnerships");

            migrationBuilder.DropTable(
                name: "PropertyPhotos");

            migrationBuilder.DropTable(
                name: "PropertyRooms");

            migrationBuilder.DropTable(
                name: "PropertyWorkflowHistory");

            migrationBuilder.DropTable(
                name: "Streets");

            migrationBuilder.DropTable(
                name: "PropertyUnits");

            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropTable(
                name: "PropertyFloors");

            migrationBuilder.DropTable(
                name: "SubDistricts");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
