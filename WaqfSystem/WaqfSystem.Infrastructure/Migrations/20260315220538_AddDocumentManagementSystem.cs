using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaqfSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyDocuments");

            migrationBuilder.CreateTable(
                name: "PropertyDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, collation: "Arabic_CI_AS"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, collation: "Arabic_CI_AS"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, collation: "Arabic_CI_AS"),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, collation: "Arabic_CI_AS"),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentVersionId = table.Column<long>(type: "bigint", nullable: true),
                    VersionCount = table.Column<int>(type: "int", nullable: false),
                    LinkedUnitId = table.Column<int>(type: "int", nullable: true),
                    LinkedPartnershipId = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryResponsibleId = table.Column<int>(type: "int", nullable: true),
                    VerifiedById = table.Column<int>(type: "int", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, collation: "Arabic_CI_AS"),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    OcrText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcrConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    OcrProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Alert1Sent = table.Column<bool>(type: "bit", nullable: false),
                    Alert2Sent = table.Column<bool>(type: "bit", nullable: false),
                    ExpiredAlertSent = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<int>(type: "int", nullable: true),
                    DeletedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAlerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    AlertLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AlertType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DaysRemaining = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadByUserId = table.Column<int>(type: "int", nullable: true),
                    NotifiedUserIds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAlerts_PropertyDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "PropertyDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DocumentAlerts_Users_ReadByUserId",
                        column: x => x.ReadByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentResponsibles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AssignedById = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true, collation: "Arabic_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentResponsibles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentResponsibles_PropertyDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "PropertyDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DocumentResponsibles_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentResponsibles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Arabic_CI_AS"),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS"),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    HasExpiry = table.Column<bool>(type: "bit", nullable: false),
                    AlertDays1 = table.Column<int>(type: "int", nullable: true),
                    AlertDays2 = table.Column<int>(type: "int", nullable: true),
                    AllowedExtensions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxFileSizeMB = table.Column<int>(type: "int", nullable: false),
                    VerifierRoles = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    UploadedById = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Arabic_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_PropertyDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "PropertyDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAuditTrail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionByUserId = table.Column<int>(type: "int", nullable: true),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    VersionId = table.Column<long>(type: "bigint", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, collation: "Arabic_CI_AS"),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAuditTrail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAuditTrail_DocumentVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentAuditTrail_PropertyDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "PropertyDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DocumentAuditTrail_Users_ActionByUserId",
                        column: x => x.ActionByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_PropDoc_Expiry",
                table: "PropertyDocuments",
                columns: new[] { "ExpiryDate", "Status", "IsDeleted" },
                filter: "[ExpiryDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PropDoc_Property",
                table: "PropertyDocuments",
                columns: new[] { "PropertyId", "IsDeleted", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PropDoc_Responsible",
                table: "PropertyDocuments",
                columns: new[] { "PrimaryResponsibleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PropDoc_Type",
                table: "PropertyDocuments",
                columns: new[] { "DocumentTypeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_CurrentVersionId",
                table: "PropertyDocuments",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_LinkedUnitId",
                table: "PropertyDocuments",
                column: "LinkedUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DocAlert_Doc",
                table: "DocumentAlerts",
                columns: new[] { "DocumentId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_DocAlert_Level",
                table: "DocumentAlerts",
                columns: new[] { "AlertLevel", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAlerts_ReadByUserId",
                table: "DocumentAlerts",
                column: "ReadByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocAudit_Doc",
                table: "DocumentAuditTrail",
                columns: new[] { "DocumentId", "ActionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DocAudit_Property",
                table: "DocumentAuditTrail",
                columns: new[] { "PropertyId", "ActionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAuditTrail_ActionByUserId",
                table: "DocumentAuditTrail",
                column: "ActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAuditTrail_VersionId",
                table: "DocumentAuditTrail",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocResponsible_User",
                table: "DocumentResponsibles",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResponsibles_AssignedById",
                table: "DocumentResponsibles",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResponsibles_DocumentId_UserId",
                table: "DocumentResponsibles",
                columns: new[] { "DocumentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId_VersionNumber",
                table: "DocumentVersions",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_UploadedById",
                table: "DocumentVersions",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_DocVersion_Doc",
                table: "DocumentVersions",
                columns: new[] { "DocumentId", "IsCurrent" });

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyDocuments_DocumentTypes_DocumentTypeId",
                table: "PropertyDocuments",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyDocuments_DocumentVersions_CurrentVersionId",
                table: "PropertyDocuments",
                column: "CurrentVersionId",
                principalTable: "DocumentVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyDocuments_PropertyUnits_LinkedUnitId",
                table: "PropertyDocuments",
                column: "LinkedUnitId",
                principalTable: "PropertyUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyDocuments_Users_PrimaryResponsibleId",
                table: "PropertyDocuments",
                column: "PrimaryResponsibleId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyDocuments_DocumentTypes_DocumentTypeId",
                table: "PropertyDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyDocuments_DocumentVersions_CurrentVersionId",
                table: "PropertyDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyDocuments_PropertyUnits_LinkedUnitId",
                table: "PropertyDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyDocuments_Users_PrimaryResponsibleId",
                table: "PropertyDocuments");

            migrationBuilder.DropTable(
                name: "DocumentAlerts");

            migrationBuilder.DropTable(
                name: "DocumentAuditTrail");

            migrationBuilder.DropTable(
                name: "DocumentResponsibles");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "DocumentVersions");

            migrationBuilder.DropIndex(
                name: "IX_PropDoc_Expiry",
                table: "PropertyDocuments");

            migrationBuilder.DropIndex(
                name: "IX_PropDoc_Property",
                table: "PropertyDocuments");

            migrationBuilder.DropIndex(
                name: "IX_PropDoc_Responsible",
                table: "PropertyDocuments");

            migrationBuilder.DropIndex(
                name: "IX_PropDoc_Type",
                table: "PropertyDocuments");

            migrationBuilder.DropIndex(
                name: "IX_PropertyDocuments_CurrentVersionId",
                table: "PropertyDocuments");

            migrationBuilder.DropIndex(
                name: "IX_PropertyDocuments_LinkedUnitId",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "Alert1Sent",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "CurrentVersionId",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedReason",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "LinkedPartnershipId",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "PropertyDocuments");

            migrationBuilder.DropColumn(
                name: "VersionCount",
                table: "PropertyDocuments");

            migrationBuilder.RenameColumn(
                name: "PrimaryResponsibleId",
                table: "PropertyDocuments",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "OcrProcessedAt",
                table: "PropertyDocuments",
                newName: "DocumentDate");

            migrationBuilder.RenameColumn(
                name: "LinkedUnitId",
                table: "PropertyDocuments",
                newName: "FileSizeKB");

            migrationBuilder.RenameColumn(
                name: "ExpiredAlertSent",
                table: "PropertyDocuments",
                newName: "IsVerified");

            migrationBuilder.RenameColumn(
                name: "Alert2Sent",
                table: "PropertyDocuments",
                newName: "IsOriginal");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "PropertyDocuments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "Arabic_CI_AS");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PropertyDocuments",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<byte>(
                name: "DocumentCategory",
                table: "PropertyDocuments",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "PropertyDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<byte>(
                name: "FileFormat",
                table: "PropertyDocuments",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "PropertyDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GisAttachedLayerId",
                table: "PropertyDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuingCity",
                table: "PropertyDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PropertyDocuments",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Arabic_CI_AS");

            migrationBuilder.AddColumn<byte>(
                name: "VerificationMethod",
                table: "PropertyDocuments",
                type: "tinyint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1486));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1635));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1639));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1640));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1641));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1642));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1643));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1644));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1645));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1646));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1647));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1648));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1648));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1649));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1650));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1651));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1652));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1653));

            migrationBuilder.UpdateData(
                table: "Governorates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1660));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1704));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1709));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1711));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1712));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1713));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1713));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1717));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1718));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1719));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 15, 12, 18, 27, 489, DateTimeKind.Utc).AddTicks(1719));

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

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyDocuments_Users_UpdatedById",
                table: "PropertyDocuments",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
