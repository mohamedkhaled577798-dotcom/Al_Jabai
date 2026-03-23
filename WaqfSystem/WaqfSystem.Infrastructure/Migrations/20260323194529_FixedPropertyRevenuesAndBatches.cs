using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedPropertyRevenuesAndBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PartnershipType",
                table: "PropertyPartnerships");

            migrationBuilder.AddColumn<string>(
                name: "CustomCalculationFormula",
                table: "PropertyPartnerships",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "CustomPartnershipName",
                table: "PropertyPartnerships",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "ExpenseBearingMethod",
                table: "PropertyPartnerships",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<decimal>(
                name: "NetRevenue",
                table: "PartnerRevenueDistributions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalExpenses",
                table: "PartnerRevenueDistributions",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CollectionBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CollectedById = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCount = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionBatches_Users_CollectedById",
                        column: x => x.CollectedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionBatches_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionBatches_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionSmartLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SuggestionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    FloorId = table.Column<int>(type: "int", nullable: true),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    WasActedOn = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSmartLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSmartLogs_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionSmartLogs_PropertyFloors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "PropertyFloors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionSmartLogs_PropertyUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionSmartLogs_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionSmartLogs_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionSmartLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnershipConditionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, collation: "Arabic_CI_AS"),
                    Scope = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, collation: "Arabic_CI_AS"),
                    RuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    FixedAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    PercentValue = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MinRevenueThreshold = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MaxRevenueThreshold = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DistributionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true, collation: "Arabic_CI_AS"),
                    SeasonLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true, collation: "Arabic_CI_AS"),
                    PriorityOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnershipConditionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnershipConditionRules_PropertyPartnerships_PartnershipId",
                        column: x => x.PartnershipId,
                        principalTable: "PropertyPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnershipConditionRules_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnershipExpenseEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Arabic_CI_AS"),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false, collation: "Arabic_CI_AS"),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnershipExpenseEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnershipExpenseEntries_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnershipExpenseEntries_PropertyPartnerships_PartnershipId",
                        column: x => x.PartnershipId,
                        principalTable: "PropertyPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnershipExpenseEntries_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FloorId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    TenantNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantNationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RentAmount = table.Column<decimal>(type: "decimal(15,2)", precision: 18, scale: 2, nullable: false),
                    InsuranceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualTerminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GracePeriodDays = table.Column<byte>(type: "tinyint", nullable: false),
                    PenaltyPerDay = table.Column<decimal>(type: "decimal(15,2)", precision: 18, scale: 2, nullable: true),
                    AllowsPartialPayments = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ContractFileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Arabic_CI_AS"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentContracts_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentContracts_PropertyFloors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "PropertyFloors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentContracts_PropertyUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentContracts_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentContracts_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RevenuePeriodLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FloorId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    PeriodLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LockedLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    ReasonAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LockedByRevenueCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenuePeriodLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevenuePeriodLocks_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevenuePeriodLocks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevenuePeriodLocks_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyRevenues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    SuggestedBySystem = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VarianceApprovedBy = table.Column<int>(type: "int", nullable: true),
                    VarianceApprovalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FloorId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    CollectedById = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PeriodLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayerNameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectionLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    RevenueCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyRevenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_CollectionBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "CollectionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_PropertyFloors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "PropertyFloors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_PropertyUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_RentContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "RentContracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_Users_CollectedById",
                        column: x => x.CollectedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyRevenues_Users_VarianceApprovedBy",
                        column: x => x.VarianceApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentPaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(15,2)", precision: 18, scale: 2, nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentPaymentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentPaymentSchedules_RentContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "RentContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentPaymentSchedules_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentPaymentSchedules_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1198));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1348));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1352));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1353));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1354));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1355));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1355));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1400));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1401));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1402));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1403));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1404));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1405));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1406));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1407));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1408));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1409));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1410));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1411));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1455));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1459));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1460));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1461));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1462));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1463));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1464));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 23, 19, 45, 27, 710, DateTimeKind.Utc).AddTicks(1466));

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExpenseBearingMethod",
                table: "PropertyPartnerships",
                sql: "[ExpenseBearingMethod] IN ('BeforeDistribution','SharedByPercent','WaqfOnly','PartnerOnly')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PartnershipType",
                table: "PropertyPartnerships",
                sql: "[PartnershipType] IN ('RevenuePercent','FloorOwnership','UnitOwnership','UsufructRight','LandPercent','TimedPartnership','HarvestShare','Custom')");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBatches_CollectedById",
                table: "CollectionBatches",
                column: "CollectedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBatches_CreatedById",
                table: "CollectionBatches",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBatches_UpdatedById",
                table: "CollectionBatches",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSmartLogs_CreatedById",
                table: "CollectionSmartLogs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSmartLogs_FloorId",
                table: "CollectionSmartLogs",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSmartLogs_PropertyId",
                table: "CollectionSmartLogs",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSmartLogs_UnitId",
                table: "CollectionSmartLogs",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSmartLogs_UpdatedById",
                table: "CollectionSmartLogs",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSmartLogs_UserId",
                table: "CollectionSmartLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionRule_Partnership_Active",
                table: "PartnershipConditionRules",
                columns: new[] { "PartnershipId", "IsActive", "PriorityOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnershipConditionRules_CreatedById",
                table: "PartnershipConditionRules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_Partnership_Period",
                table: "PartnershipExpenseEntries",
                columns: new[] { "PartnershipId", "PeriodStartDate", "PeriodEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnershipExpenseEntries_CreatedById",
                table: "PartnershipExpenseEntries",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartnershipExpenseEntries_PropertyId",
                table: "PartnershipExpenseEntries",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_BatchId",
                table: "PropertyRevenues",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_CollectedById",
                table: "PropertyRevenues",
                column: "CollectedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_ContractId",
                table: "PropertyRevenues",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_CreatedById",
                table: "PropertyRevenues",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_FloorId",
                table: "PropertyRevenues",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_PropertyId",
                table: "PropertyRevenues",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_UnitId",
                table: "PropertyRevenues",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_UpdatedById",
                table: "PropertyRevenues",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRevenues_VarianceApprovedBy",
                table: "PropertyRevenues",
                column: "VarianceApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RentContracts_CreatedById",
                table: "RentContracts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RentContracts_FloorId",
                table: "RentContracts",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_RentContracts_PropertyId",
                table: "RentContracts",
                column: "PropertyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RentContracts_Status",
                table: "RentContracts",
                column: "Status",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RentContracts_UnitId",
                table: "RentContracts",
                column: "UnitId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RentContracts_UpdatedById",
                table: "RentContracts",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RentPaymentSchedules_ContractId_PeriodLabel",
                table: "RentPaymentSchedules",
                columns: new[] { "ContractId", "PeriodLabel" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RentPaymentSchedules_CreatedById",
                table: "RentPaymentSchedules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RentPaymentSchedules_UpdatedById",
                table: "RentPaymentSchedules",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RevenuePeriodLocks_CreatedById",
                table: "RevenuePeriodLocks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RevenuePeriodLocks_PropertyId_FloorId_UnitId_PeriodLabel",
                table: "RevenuePeriodLocks",
                columns: new[] { "PropertyId", "FloorId", "UnitId", "PeriodLabel" },
                unique: true,
                filter: "[FloorId] IS NOT NULL AND [UnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RevenuePeriodLocks_UpdatedById",
                table: "RevenuePeriodLocks",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionSmartLogs");

            migrationBuilder.DropTable(
                name: "PartnershipConditionRules");

            migrationBuilder.DropTable(
                name: "PartnershipExpenseEntries");

            migrationBuilder.DropTable(
                name: "PropertyRevenues");

            migrationBuilder.DropTable(
                name: "RentPaymentSchedules");

            migrationBuilder.DropTable(
                name: "RevenuePeriodLocks");

            migrationBuilder.DropTable(
                name: "CollectionBatches");

            migrationBuilder.DropTable(
                name: "RentContracts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExpenseBearingMethod",
                table: "PropertyPartnerships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PartnershipType",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "CustomCalculationFormula",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "CustomPartnershipName",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "ExpenseBearingMethod",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "NetRevenue",
                table: "PartnerRevenueDistributions");

            migrationBuilder.DropColumn(
                name: "TotalExpenses",
                table: "PartnerRevenueDistributions");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9550));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9777));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9793));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9796));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9797));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9799));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9800));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9801));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9803));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9804));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9805));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9806));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9808));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9809));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9810));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9812));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9813));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9814));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9853));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9910));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9937));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9939));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9940));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9942));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9943));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9944));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9946));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9947));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 22, 5, 36, 590, DateTimeKind.Utc).AddTicks(9949));

            migrationBuilder.AddCheckConstraint(
                name: "CK_PartnershipType",
                table: "PropertyPartnerships",
                sql: "[PartnershipType] IN ('RevenuePercent','FloorOwnership','UnitOwnership','UsufructRight','LandPercent','TimedPartnership','HarvestShare')");
        }
    }
}
