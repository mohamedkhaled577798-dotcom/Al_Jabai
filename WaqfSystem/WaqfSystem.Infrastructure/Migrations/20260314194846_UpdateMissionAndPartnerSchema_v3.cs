using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMissionAndPartnerSchema_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_Users_AssignedToId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_Users_SupervisorId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_Users_UpdatedById",
                table: "InspectionMissions");
            */

            /*
            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_AssignedToId",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_GovernorateId",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_MissionNumber",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_ScheduledDate",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_Status",
                table: "InspectionMissions");
            */

            migrationBuilder.DropColumn(
                name: "MissionNumber",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InspectionMissions");

            migrationBuilder.RenameColumn(
                name: "CheckInAt",
                table: "InspectionMissions",
                newName: "CheckinAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "InspectionMissions",
                newName: "SubDistrictId");

            migrationBuilder.RenameColumn(
                name: "SupervisorId",
                table: "InspectionMissions",
                newName: "ReviewerUserId");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "InspectionMissions",
                newName: "SubmittedAt");

            migrationBuilder.RenameColumn(
                name: "ScheduledDate",
                table: "InspectionMissions",
                newName: "MissionDate");

            migrationBuilder.RenameColumn(
                name: "CompletedPropertyCount",
                table: "InspectionMissions",
                newName: "ReviewedPropertyCount");

            migrationBuilder.RenameColumn(
                name: "CheckInLongitude",
                table: "InspectionMissions",
                newName: "CheckinLng");

            migrationBuilder.RenameColumn(
                name: "CheckInLatitude",
                table: "InspectionMissions",
                newName: "CheckinLat");

            migrationBuilder.RenameColumn(
                name: "AssignedToId",
                table: "InspectionMissions",
                newName: "ChecklistTemplateId");

            migrationBuilder.RenameColumn(
                name: "AssignedTeamId",
                table: "InspectionMissions",
                newName: "AssignedToUserId");

            /*
            migrationBuilder.RenameIndex(
                name: "IX_InspectionMissions_UpdatedById",
                table: "InspectionMissions",
                newName: "IX_InspectionMissions_SubDistrictId");

            migrationBuilder.RenameIndex(
                name: "IX_InspectionMissions_SupervisorId",
                table: "InspectionMissions",
                newName: "IX_InspectionMissions_ReviewerUserId");
            */

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "InspectionMissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AlterColumn<int>(
                name: "TargetPropertyCount",
                table: "InspectionMissions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MissionType",
                table: "InspectionMissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "GovernorateId",
                table: "InspectionMissions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InspectionMissions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "InspectionMissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualCompletionDate",
                table: "InspectionMissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedPropertyCount",
                table: "InspectionMissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "InspectionMissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedByUserId",
                table: "InspectionMissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToTeamId",
                table: "InspectionMissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentNotes",
                table: "InspectionMissions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageDqsScore",
                table: "InspectionMissions",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "InspectionMissions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "CorrectionNotes",
                table: "InspectionMissions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentStageChangedAt",
                table: "InspectionMissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "EnteredPropertyCount",
                table: "InspectionMissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedCompletionDate",
                table: "InspectionMissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUrgent",
                table: "InspectionMissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MissionCode",
                table: "InspectionMissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "InspectionMissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "InspectionMissions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "InspectionMissions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "InspectionMissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetArea",
                table: "InspectionMissions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.CreateTable(
                name: "InspectionTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Arabic_CI_AS"),
                    TeamCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    LeaderId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTeams_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionTeams_Users_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionChecklistTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Arabic_CI_AS"),
                    MissionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionChecklistTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionChecklistTemplates_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionPropertyEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    LocalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: false),
                    EntryStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DqsAtEntry = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EntryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionPropertyEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionPropertyEntries_InspectionMissions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "InspectionMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionPropertyEntries_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionPropertyEntries_Users_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionPropertyEntries_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionStageHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    FromStage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToStage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedById = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    TriggerAction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionStageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionStageHistory_InspectionMissions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "InspectionMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionStageHistory_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankIBAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partners_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Partners_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InspectionTeamMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionTeamMembers_InspectionTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "InspectionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionTeamMembers_Users_AddedById",
                        column: x => x.AddedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionTeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionChecklistResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: false),
                    Results = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletionPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionChecklistResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionChecklistResults_InspectionMissions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "InspectionMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionChecklistResults_MissionChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "MissionChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MissionChecklistResults_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_AssignedByUserId",
                table: "InspectionMissions",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_AssignedToTeamId",
                table: "InspectionMissions",
                column: "AssignedToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_AssignedToUserId_Stage",
                table: "InspectionMissions",
                columns: new[] { "AssignedToUserId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_ChecklistTemplateId",
                table: "InspectionMissions",
                column: "ChecklistTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_GovernorateId_MissionDate",
                table: "InspectionMissions",
                columns: new[] { "GovernorateId", "MissionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_MissionCode",
                table: "InspectionMissions",
                column: "MissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_MissionDate_ExpectedCompletionDate",
                table: "InspectionMissions",
                columns: new[] { "MissionDate", "ExpectedCompletionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionMissions_Stage_MissionDate",
                table: "InspectionMissions",
                columns: new[] { "Stage", "MissionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTeamMembers_AddedById",
                table: "InspectionTeamMembers",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTeamMembers_TeamId_UserId",
                table: "InspectionTeamMembers",
                columns: new[] { "TeamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTeamMembers_UserId_IsActive",
                table: "InspectionTeamMembers",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTeams_GovernorateId_IsActive",
                table: "InspectionTeams",
                columns: new[] { "GovernorateId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTeams_LeaderId",
                table: "InspectionTeams",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTeams_TeamCode",
                table: "InspectionTeams",
                column: "TeamCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionChecklistResults_CompletedByUserId",
                table: "MissionChecklistResults",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionChecklistResults_MissionId",
                table: "MissionChecklistResults",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionChecklistResults_TemplateId",
                table: "MissionChecklistResults",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionChecklistTemplates_CreatedById",
                table: "MissionChecklistTemplates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MissionPropertyEntries_EnteredByUserId",
                table: "MissionPropertyEntries",
                column: "EnteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionPropertyEntries_MissionId_EntryStatus",
                table: "MissionPropertyEntries",
                columns: new[] { "MissionId", "EntryStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_MissionPropertyEntries_MissionId_PropertyId",
                table: "MissionPropertyEntries",
                columns: new[] { "MissionId", "PropertyId" },
                unique: true,
                filter: "[PropertyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MissionPropertyEntries_PropertyId",
                table: "MissionPropertyEntries",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionPropertyEntries_ReviewedByUserId",
                table: "MissionPropertyEntries",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionStageHistory_ChangedById",
                table: "MissionStageHistory",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_MissionStageHistory_MissionId_ChangedAt",
                table: "MissionStageHistory",
                columns: new[] { "MissionId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_CreatedById",
                table: "Partners",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_UpdatedById",
                table: "Partners",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_InspectionTeams_AssignedToTeamId",
                table: "InspectionMissions",
                column: "AssignedToTeamId",
                principalTable: "InspectionTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_MissionChecklistTemplates_ChecklistTemplateId",
                table: "InspectionMissions",
                column: "ChecklistTemplateId",
                principalTable: "MissionChecklistTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_SubDistricts_SubDistrictId",
                table: "InspectionMissions",
                column: "SubDistrictId",
                principalTable: "SubDistricts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_Users_AssignedByUserId",
                table: "InspectionMissions",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_Users_AssignedToUserId",
                table: "InspectionMissions",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_Users_ReviewerUserId",
                table: "InspectionMissions",
                column: "ReviewerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_InspectionTeams_AssignedToTeamId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_MissionChecklistTemplates_ChecklistTemplateId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_SubDistricts_SubDistrictId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_Users_AssignedByUserId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_Users_AssignedToUserId",
                table: "InspectionMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionMissions_Users_ReviewerUserId",
                table: "InspectionMissions");

            migrationBuilder.DropTable(
                name: "InspectionTeamMembers");

            migrationBuilder.DropTable(
                name: "MissionChecklistResults");

            migrationBuilder.DropTable(
                name: "MissionPropertyEntries");

            migrationBuilder.DropTable(
                name: "MissionStageHistory");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "InspectionTeams");

            migrationBuilder.DropTable(
                name: "MissionChecklistTemplates");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_AssignedByUserId",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_AssignedToTeamId",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_AssignedToUserId_Stage",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_ChecklistTemplateId",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_GovernorateId_MissionDate",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_MissionCode",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_MissionDate_ExpectedCompletionDate",
                table: "InspectionMissions");

            migrationBuilder.DropIndex(
                name: "IX_InspectionMissions_Stage_MissionDate",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "ActualCompletionDate",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "ApprovedPropertyCount",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "AssignedToTeamId",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "AssignmentNotes",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "AverageDqsScore",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "CorrectionNotes",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "CurrentStageChangedAt",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "EnteredPropertyCount",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "ExpectedCompletionDate",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "IsUrgent",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "MissionCode",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "InspectionMissions");

            migrationBuilder.DropColumn(
                name: "TargetArea",
                table: "InspectionMissions");

            migrationBuilder.RenameColumn(
                name: "CheckinAt",
                table: "InspectionMissions",
                newName: "CheckInAt");

            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                table: "InspectionMissions",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "SubDistrictId",
                table: "InspectionMissions",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "ReviewerUserId",
                table: "InspectionMissions",
                newName: "SupervisorId");

            migrationBuilder.RenameColumn(
                name: "ReviewedPropertyCount",
                table: "InspectionMissions",
                newName: "CompletedPropertyCount");

            migrationBuilder.RenameColumn(
                name: "MissionDate",
                table: "InspectionMissions",
                newName: "ScheduledDate");

            migrationBuilder.RenameColumn(
                name: "ChecklistTemplateId",
                table: "InspectionMissions",
                newName: "AssignedToId");

            migrationBuilder.RenameColumn(
                name: "CheckinLng",
                table: "InspectionMissions",
                newName: "CheckInLongitude");

            migrationBuilder.RenameColumn(
                name: "CheckinLat",
                table: "InspectionMissions",
                newName: "CheckInLatitude");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId",
                table: "InspectionMissions",
                newName: "AssignedTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_InspectionMissions_SubDistrictId",
                table: "InspectionMissions",
                newName: "IX_InspectionMissions_UpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_InspectionMissions_ReviewerUserId",
                table: "InspectionMissions",
                newName: "IX_InspectionMissions_SupervisorId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "InspectionMissions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AlterColumn<int>(
                name: "TargetPropertyCount",
                table: "InspectionMissions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "MissionType",
                table: "InspectionMissions",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<int>(
                name: "GovernorateId",
                table: "InspectionMissions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InspectionMissions",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Arabic_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "MissionNumber",
                table: "InspectionMissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "InspectionMissions",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "InspectionMissions",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

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
                name: "IX_InspectionMissions_AssignedToId",
                table: "InspectionMissions",
                column: "AssignedToId",
                filter: "[IsDeleted] = 0");

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

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_Users_AssignedToId",
                table: "InspectionMissions",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_Users_SupervisorId",
                table: "InspectionMissions",
                column: "SupervisorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionMissions_Users_UpdatedById",
                table: "InspectionMissions",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
