using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace WaqfGIS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newgis1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EastStreet",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasLegalCase",
                table: "WaqfProperties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisputed",
                table: "WaqfProperties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MainAccessRoad",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NorthStreet",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerSqm",
                table: "WaqfProperties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SouthStreet",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WestStreet",
                table: "WaqfProperties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasInvestmentContract",
                table: "WaqfLands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLegalCase",
                table: "WaqfLands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisputed",
                table: "WaqfLands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerSqm",
                table: "WaqfLands",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvestmentContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContractDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractPurpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvestorType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorIdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    DurationYears = table.Column<int>(type: "int", nullable: false),
                    IsRenewable = table.Column<bool>(type: "bit", nullable: false),
                    RenewalNoticeDays = table.Column<int>(type: "int", nullable: true),
                    RenewalTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDayOfMonth = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GuaranteeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuaranteeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasAnnualIncrease = table.Column<bool>(type: "bit", nullable: false),
                    AnnualIncreasePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualIncreaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaintenanceResponsibility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtilitiesResponsibility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowedUsage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProhibitedUsage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowSubleasing = table.Column<bool>(type: "bit", nullable: false),
                    LatePaymentPenaltyDaily = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LatePaymentPenaltyPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EarlyTerminationPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ContractBreachPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotifyBeforeSixMonths = table.Column<bool>(type: "bit", nullable: false),
                    NotifyBeforeThreeMonths = table.Column<bool>(type: "bit", nullable: false),
                    NotifyBeforeOneMonth = table.Column<bool>(type: "bit", nullable: false),
                    LastNotificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalPaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentsCount = table.Column<int>(type: "int", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsibleOfficer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsibleOfficerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaqfLandId = table.Column<int>(type: "int", nullable: true),
                    WaqfPropertyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentContracts_WaqfLands_WaqfLandId",
                        column: x => x.WaqfLandId,
                        principalTable: "WaqfLands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvestmentContracts_WaqfProperties_WaqfPropertyId",
                        column: x => x.WaqfPropertyId,
                        principalTable: "WaqfProperties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LegalDisputes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourtName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourtType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlaintiffName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaintiffPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaintiffAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefendantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefendantPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefendantAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisputeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisputeSubject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisputeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CaseStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastHearingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextHearingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProcedure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasVerdict = table.Column<bool>(type: "bit", nullable: false),
                    VerdictDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerdictSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerdictResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAppealed = table.Column<bool>(type: "bit", nullable: false),
                    AppealDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsExecuted = table.Column<bool>(type: "bit", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LawyerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LawyerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LawyerLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LegalCosts = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WaqfLandId = table.Column<int>(type: "int", nullable: true),
                    WaqfPropertyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDisputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalDisputes_WaqfLands_WaqfLandId",
                        column: x => x.WaqfLandId,
                        principalTable: "WaqfLands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LegalDisputes_WaqfProperties_WaqfPropertyId",
                        column: x => x.WaqfPropertyId,
                        principalTable: "WaqfProperties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComparisonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComparisonDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyComparisons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyPricings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    SubDistrictId = table.Column<int>(type: "int", nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyTypeId = table.Column<int>(type: "int", nullable: false),
                    PropertySubType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PriceSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinPricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxPricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AvgPricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LocationQuality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProximityToMainRoad = table.Column<int>(type: "int", nullable: true),
                    NearPublicServices = table.Column<bool>(type: "bit", nullable: false),
                    DemandLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketTrend = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyPricings_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyPricings_PropertyTypes_PropertyTypeId",
                        column: x => x.PropertyTypeId,
                        principalTable: "PropertyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyPricings_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyPricings_SubDistricts_SubDistrictId",
                        column: x => x.SubDistrictId,
                        principalTable: "SubDistricts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyServiceAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallRating = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasElectricity = table.Column<bool>(type: "bit", nullable: false),
                    ElectricityDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ElectricityScore = table.Column<int>(type: "int", nullable: true),
                    HasWater = table.Column<bool>(type: "bit", nullable: false),
                    WaterDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WaterScore = table.Column<int>(type: "int", nullable: true),
                    HasGas = table.Column<bool>(type: "bit", nullable: false),
                    GasDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GasScore = table.Column<int>(type: "int", nullable: true),
                    HasSewage = table.Column<bool>(type: "bit", nullable: false),
                    SewageDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SewageScore = table.Column<int>(type: "int", nullable: true),
                    NearbyHospitalsCount = table.Column<int>(type: "int", nullable: false),
                    NearestHospitalDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HealthServicesScore = table.Column<int>(type: "int", nullable: true),
                    NearbySchoolsCount = table.Column<int>(type: "int", nullable: false),
                    NearestSchoolDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EducationServicesScore = table.Column<int>(type: "int", nullable: true),
                    NearbyPoliceStationsCount = table.Column<int>(type: "int", nullable: false),
                    NearestPoliceStationDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NearbyFireStationsCount = table.Column<int>(type: "int", nullable: false),
                    NearestFireStationDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SafetyServicesScore = table.Column<int>(type: "int", nullable: true),
                    NearbyFuelStationsCount = table.Column<int>(type: "int", nullable: false),
                    NearestFuelStationDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TransportServicesScore = table.Column<int>(type: "int", nullable: true),
                    NearbyMarketsCount = table.Column<int>(type: "int", nullable: false),
                    NearestMarketDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NearbyBanksCount = table.Column<int>(type: "int", nullable: false),
                    NearestBankDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CommercialServicesScore = table.Column<int>(type: "int", nullable: true),
                    OnMainRoad = table.Column<bool>(type: "bit", nullable: false),
                    DistanceToMainRoad = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RoadCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessibilityScore = table.Column<int>(type: "int", nullable: true),
                    Strengths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weaknesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyServiceAssessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<Point>(type: "geometry", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Boundary = table.Column<Polygon>(type: "geometry", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    SubDistrictId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Capacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CapacityUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ServiceRadius = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkingHours = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Is24Hours = table.Column<bool>(type: "bit", nullable: false),
                    WorkingDays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QualityRating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ServiceQuality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceFacilities_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceFacilities_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceFacilities_SubDistricts_SubDistrictId",
                        column: x => x.SubDistrictId,
                        principalTable: "SubDistricts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContractDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractDocuments_InvestmentContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "InvestmentContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    PaymentNumber = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DaysLate = table.Column<int>(type: "int", nullable: true),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractPayments_InvestmentContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "InvestmentContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisputeDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisputeId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisputeDocuments_LegalDisputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "LegalDisputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyComparisonItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComparisonId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerSqm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationScore = table.Column<int>(type: "int", nullable: true),
                    ServicesScore = table.Column<int>(type: "int", nullable: true),
                    AccessibilityScore = table.Column<int>(type: "int", nullable: true),
                    OverallScore = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyComparisonItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyComparisonItems_PropertyComparisons_ComparisonId",
                        column: x => x.ComparisonId,
                        principalTable: "PropertyComparisons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceFacilityId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceImages_ServiceFacilities_ServiceFacilityId",
                        column: x => x.ServiceFacilityId,
                        principalTable: "ServiceFacilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProximities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    ServiceFacilityId = table.Column<int>(type: "int", nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TravelTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    AccessMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProximities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProximities_PropertyServiceAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "PropertyServiceAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceProximities_ServiceFacilities_ServiceFacilityId",
                        column: x => x.ServiceFacilityId,
                        principalTable: "ServiceFacilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_ContractId",
                table: "ContractDocuments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPayments_ContractId",
                table: "ContractPayments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPayments_DueDate",
                table: "ContractPayments",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPayments_Status",
                table: "ContractPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeDocuments_DisputeId",
                table: "DisputeDocuments",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentContracts_ContractNumber",
                table: "InvestmentContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentContracts_EndDate",
                table: "InvestmentContracts",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentContracts_EntityType_EntityId",
                table: "InvestmentContracts",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentContracts_WaqfLandId",
                table: "InvestmentContracts",
                column: "WaqfLandId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentContracts_WaqfPropertyId",
                table: "InvestmentContracts",
                column: "WaqfPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDisputes_CaseNumber",
                table: "LegalDisputes",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDisputes_EntityType_EntityId",
                table: "LegalDisputes",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_LegalDisputes_WaqfLandId",
                table: "LegalDisputes",
                column: "WaqfLandId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDisputes_WaqfPropertyId",
                table: "LegalDisputes",
                column: "WaqfPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyComparisonItems_ComparisonId",
                table: "PropertyComparisonItems",
                column: "ComparisonId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyComparisons_ComparisonDate",
                table: "PropertyComparisons",
                column: "ComparisonDate");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPricings_DistrictId",
                table: "PropertyPricings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPricings_PriceDate",
                table: "PropertyPricings",
                column: "PriceDate");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPricings_PropertyTypeId",
                table: "PropertyPricings",
                column: "PropertyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPricings_ProvinceId_DistrictId_PropertyTypeId",
                table: "PropertyPricings",
                columns: new[] { "ProvinceId", "DistrictId", "PropertyTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPricings_SubDistrictId",
                table: "PropertyPricings",
                column: "SubDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyServiceAssessments_AssessmentDate",
                table: "PropertyServiceAssessments",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyServiceAssessments_EntityType_EntityId",
                table: "PropertyServiceAssessments",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFacilities_Code",
                table: "ServiceFacilities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFacilities_DistrictId",
                table: "ServiceFacilities",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFacilities_ProvinceId",
                table: "ServiceFacilities",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFacilities_SubDistrictId",
                table: "ServiceFacilities",
                column: "SubDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImages_ServiceFacilityId",
                table: "ServiceImages",
                column: "ServiceFacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProximities_AssessmentId",
                table: "ServiceProximities",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProximities_ServiceFacilityId",
                table: "ServiceProximities",
                column: "ServiceFacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractDocuments");

            migrationBuilder.DropTable(
                name: "ContractPayments");

            migrationBuilder.DropTable(
                name: "DisputeDocuments");

            migrationBuilder.DropTable(
                name: "PropertyComparisonItems");

            migrationBuilder.DropTable(
                name: "PropertyPricings");

            migrationBuilder.DropTable(
                name: "ServiceImages");

            migrationBuilder.DropTable(
                name: "ServiceProximities");

            migrationBuilder.DropTable(
                name: "InvestmentContracts");

            migrationBuilder.DropTable(
                name: "LegalDisputes");

            migrationBuilder.DropTable(
                name: "PropertyComparisons");

            migrationBuilder.DropTable(
                name: "PropertyServiceAssessments");

            migrationBuilder.DropTable(
                name: "ServiceFacilities");

            migrationBuilder.DropColumn(
                name: "EastStreet",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "HasLegalCase",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "IsDisputed",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "MainAccessRoad",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "NorthStreet",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "PricePerSqm",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "SouthStreet",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "WestStreet",
                table: "WaqfProperties");

            migrationBuilder.DropColumn(
                name: "HasInvestmentContract",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "HasLegalCase",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "IsDisputed",
                table: "WaqfLands");

            migrationBuilder.DropColumn(
                name: "PricePerSqm",
                table: "WaqfLands");
        }
    }
}
