using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPartnershipSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RevenueDistribMethod",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "PartnerType",
                table: "PropertyPartnerships",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<string>(
                name: "PartnerNationalId",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PartnerBankIBAN",
                table: "PropertyPartnerships",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(34)",
                oldMaxLength: 34,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AgreementDocUrl",
                table: "PropertyPartnerships",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgreementCourt",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "AgreementNotaryName",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "AgreementReferenceNo",
                table: "PropertyPartnerships",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeactivationReason",
                table: "PropertyPartnerships",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "FarmerName",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "FarmerNationalId",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "HarvestContractType",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<decimal>(
                name: "LandSharePercentWaqf",
                table: "PropertyPartnerships",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LandTotalDunum",
                table: "PropertyPartnerships",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDistribDate",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextDistribDate",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnedFloorNumbers",
                table: "PropertyPartnerships",
                type: "nvarchar(500)",
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "OwnedUnitIds",
                table: "PropertyPartnerships",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerAddress",
                table: "PropertyPartnerships",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerBankAccountNo",
                table: "PropertyPartnerships",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerBankBranch",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerBankName",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerEmail",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerNameEn",
                table: "PropertyPartnerships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerPhone",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerPhone2",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerRegistrationNo",
                table: "PropertyPartnerships",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "PartnerWhatsApp",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<DateTime>(
                name: "PartnershipEndDate",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PartnershipStartDate",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnershipType",
                table: "PropertyPartnerships",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<int>(
                name: "RevenueDistribDay",
                table: "PropertyPartnerships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UsufructAnnualFeePerYear",
                table: "PropertyPartnerships",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsufructEndDate",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsufructStartDate",
                table: "PropertyPartnerships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsufructTermYears",
                table: "PropertyPartnerships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaqfHarvestPercent",
                table: "PropertyPartnerships",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaqfLandDunum",
                table: "PropertyPartnerships",
                type: "decimal(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaqfSharePercent",
                table: "PropertyPartnerships",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PartnerNotificationSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    TriggerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Arabic_CI_AS"),
                    TriggerDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Channels = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerNotificationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerNotificationSchedules_PropertyPartnerships_PartnershipId",
                        column: x => x.PartnershipId,
                        principalTable: "PropertyPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnerRevenueDistributions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Arabic_CI_AS"),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistributionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, collation: "Arabic_CI_AS"),
                    TotalRevenue = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    WaqfAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    PartnerAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    WaqfPercentSnapshot = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TransferStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, collation: "Arabic_CI_AS"),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransferMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    TransferReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    TransferBankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LinkedDistributionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerRevenueDistributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerRevenueDistributions_PartnerRevenueDistributions_LinkedDistributionId",
                        column: x => x.LinkedDistributionId,
                        principalTable: "PartnerRevenueDistributions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartnerRevenueDistributions_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerRevenueDistributions_PropertyPartnerships_PartnershipId",
                        column: x => x.PartnershipId,
                        principalTable: "PropertyPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerRevenueDistributions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContactLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    ContactType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, collation: "Arabic_CI_AS"),
                    ContactDirection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, collation: "Arabic_CI_AS"),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    MessageBody = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, collation: "Arabic_CI_AS"),
                    RecipientAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentById = table.Column<int>(type: "int", nullable: false),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, collation: "Arabic_CI_AS"),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    LinkedDistributionId = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContactLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerContactLogs_PartnerRevenueDistributions_LinkedDistributionId",
                        column: x => x.LinkedDistributionId,
                        principalTable: "PartnerRevenueDistributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PartnerContactLogs_PropertyPartnerships_PartnershipId",
                        column: x => x.PartnershipId,
                        principalTable: "PropertyPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerContactLogs_Users_SentById",
                        column: x => x.SentById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9732));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9871));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9876));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9877));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9878));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9879));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9880));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9882));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9883));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9884));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9885));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9886));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9887));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9888));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9889));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9891));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9893));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9894));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9930));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9933));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9934));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9935));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9936));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9937));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9938));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9939));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9947));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 12, 22, 30, 80, DateTimeKind.Utc).AddTicks(9948));

            migrationBuilder.CreateIndex(
                name: "IX_Partnership_EndDate",
                table: "PropertyPartnerships",
                column: "PartnershipEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Partnership_Property_Active",
                table: "PropertyPartnerships",
                columns: new[] { "PropertyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Partnership_UsufructEndDate",
                table: "PropertyPartnerships",
                column: "UsufructEndDate");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DistribMethod",
                table: "PropertyPartnerships",
                sql: "[RevenueDistribMethod] IN ('Monthly','Quarterly','Annual','PerCollection')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PartnershipType",
                table: "PropertyPartnerships",
                sql: "[PartnershipType] IN ('RevenuePercent','FloorOwnership','UnitOwnership','UsufructRight','LandPercent','TimedPartnership','HarvestShare')");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLog_Partnership",
                table: "PartnerContactLogs",
                columns: new[] { "PartnershipId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactLogs_LinkedDistributionId",
                table: "PartnerContactLogs",
                column: "LinkedDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContactLogs_SentById",
                table: "PartnerContactLogs",
                column: "SentById");

            migrationBuilder.CreateIndex(
                name: "IX_NotifSchedule_Pending",
                table: "PartnerNotificationSchedules",
                columns: new[] { "TriggerDate", "IsSent" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerNotificationSchedules_PartnershipId",
                table: "PartnerNotificationSchedules",
                column: "PartnershipId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRevenueDistributions_CreatedById",
                table: "PartnerRevenueDistributions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRevenueDistributions_LinkedDistributionId",
                table: "PartnerRevenueDistributions",
                column: "LinkedDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRevenueDistributions_PropertyId",
                table: "PartnerRevenueDistributions",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDistrib_Partnership",
                table: "PartnerRevenueDistributions",
                columns: new[] { "PartnershipId", "PeriodStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDistrib_Status",
                table: "PartnerRevenueDistributions",
                column: "TransferStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerContactLogs");

            migrationBuilder.DropTable(
                name: "PartnerNotificationSchedules");

            migrationBuilder.DropTable(
                name: "PartnerRevenueDistributions");

            migrationBuilder.DropIndex(
                name: "IX_Partnership_EndDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropIndex(
                name: "IX_Partnership_Property_Active",
                table: "PropertyPartnerships");

            migrationBuilder.DropIndex(
                name: "IX_Partnership_UsufructEndDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DistribMethod",
                table: "PropertyPartnerships");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PartnershipType",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "AgreementCourt",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "AgreementNotaryName",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "AgreementReferenceNo",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "DeactivationReason",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "FarmerName",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "FarmerNationalId",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "HarvestContractType",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "LandSharePercentWaqf",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "LandTotalDunum",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "LastDistribDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "NextDistribDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "OwnedFloorNumbers",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "OwnedUnitIds",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerAddress",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerBankAccountNo",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerBankBranch",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerBankName",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerEmail",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerNameEn",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerPhone",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerPhone2",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerRegistrationNo",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnerWhatsApp",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnershipEndDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnershipStartDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "PartnershipType",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "RevenueDistribDay",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "UsufructAnnualFeePerYear",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "UsufructEndDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "UsufructStartDate",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "UsufructTermYears",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "WaqfHarvestPercent",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "WaqfLandDunum",
                table: "PropertyPartnerships");

            migrationBuilder.DropColumn(
                name: "WaqfSharePercent",
                table: "PropertyPartnerships");

            migrationBuilder.AlterColumn<byte>(
                name: "RevenueDistribMethod",
                table: "PropertyPartnerships",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AlterColumn<byte>(
                name: "PartnerType",
                table: "PropertyPartnerships",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PartnerNationalId",
                table: "PropertyPartnerships",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "PartnerBankIBAN",
                table: "PropertyPartnerships",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "AgreementDocUrl",
                table: "PropertyPartnerships",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5339));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5523));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5526));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5528));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5529));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5530));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5531));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5532));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5533));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5534));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5535));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5536));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5537));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5538));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5539));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5540));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5541));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5543));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5582));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5585));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5586));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5587));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5588));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5589));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5590));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5590));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5591));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 13, 22, 29, 43, 315, DateTimeKind.Utc).AddTicks(5592));
        }
    }
}
