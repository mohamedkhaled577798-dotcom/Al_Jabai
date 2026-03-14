using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePartnershipIdsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing FKs that reference the tables being changed
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PartnerRevenueDistributions_PartnerRevenueDistributions_LinkedDistributionId') ALTER TABLE PartnerRevenueDistributions DROP CONSTRAINT FK_PartnerRevenueDistributions_PartnerRevenueDistributions_LinkedDistributionId;");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PartnerContactLogs_PartnerRevenueDistributions_LinkedDistributionId') ALTER TABLE PartnerContactLogs DROP CONSTRAINT FK_PartnerContactLogs_PartnerRevenueDistributions_LinkedDistributionId;");
            
            // Drop tables to recreate with int IDs
            migrationBuilder.DropTable(name: "PartnerNotificationSchedules");
            migrationBuilder.DropTable(name: "PartnerContactLogs");
            migrationBuilder.DropTable(name: "PartnerRevenueDistributions");

            // Recreate PartnerRevenueDistributions
            migrationBuilder.CreateTable(
                name: "PartnerRevenueDistributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistributionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaqfAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartnerAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaqfPercentSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransferStatus = table.Column<int>(type: "int", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransferMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferBankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LinkedDistributionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerRevenueDistributions", x => x.Id);
                    table.ForeignKey(name: "FK_PartnerRevenueDistributions_PropertyPartnerships_PartnershipId", column: x => x.PartnershipId, principalTable: "PropertyPartnerships", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(name: "FK_PartnerRevenueDistributions_Properties_PropertyId", column: x => x.PropertyId, principalTable: "Properties", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(name: "FK_PartnerRevenueDistributions_PartnerRevenueDistributions_LinkedDistributionId", column: x => x.LinkedDistributionId, principalTable: "PartnerRevenueDistributions", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                });

            // Recreate PartnerContactLogs
            migrationBuilder.CreateTable(
                name: "PartnerContactLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    ContactType = table.Column<int>(type: "int", nullable: false),
                    ContactDirection = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentById = table.Column<int>(type: "int", nullable: false),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedDistributionId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContactLogs", x => x.Id);
                    table.ForeignKey(name: "FK_PartnerContactLogs_PropertyPartnerships_PartnershipId", column: x => x.PartnershipId, principalTable: "PropertyPartnerships", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_PartnerContactLogs_PartnerRevenueDistributions_LinkedDistributionId", column: x => x.LinkedDistributionId, principalTable: "PartnerRevenueDistributions", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                });

            // Recreate PartnerNotificationSchedules
            migrationBuilder.CreateTable(
                name: "PartnerNotificationSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PartnershipId = table.Column<int>(type: "int", nullable: false),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    TriggerDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Channels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerNotificationSchedules", x => x.Id);
                    table.ForeignKey(name: "FK_PartnerNotificationSchedules_PropertyPartnerships_PartnershipId", column: x => x.PartnershipId, principalTable: "PropertyPartnerships", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9366));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9903));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9905));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9906));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9907));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9910));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9911));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9912));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9913));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9914));


            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9915));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9916));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9955));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9956));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9957));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9959));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9960));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 544, DateTimeKind.Utc).AddTicks(9961));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(16));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(29));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(30));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(31));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(32));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(33));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(34));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(35));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(36));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 20, 5, 38, 545, DateTimeKind.Utc).AddTicks(37));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "LinkedDistributionId",
                table: "PartnerRevenueDistributions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PartnerRevenueDistributions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PartnerNotificationSchedules",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "LinkedDistributionId",
                table: "PartnerContactLogs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PartnerContactLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(8929));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9131));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9137));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9138));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9140));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9141));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9142));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9143));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9144));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9145));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9156));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9158));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9159));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9160));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9161));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9162));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9163));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9164));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9165));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9225));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9228));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9229));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9230));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9231));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9232));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9233));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9234));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9235));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 14, 19, 48, 44, 910, DateTimeKind.Utc).AddTicks(9235));
        }
    }
}
