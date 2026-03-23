SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.DocumentResponsibles', N'U') IS NOT NULL DROP TABLE dbo.DocumentResponsibles;
IF OBJECT_ID(N'dbo.DocumentAlerts', N'U') IS NOT NULL DROP TABLE dbo.DocumentAlerts;
IF OBJECT_ID(N'dbo.DocumentAuditTrail', N'U') IS NOT NULL DROP TABLE dbo.DocumentAuditTrail;
IF OBJECT_ID(N'dbo.DocumentVersions', N'U') IS NOT NULL DROP TABLE dbo.DocumentVersions;
IF OBJECT_ID(N'dbo.PropertyDocuments', N'U') IS NOT NULL DROP TABLE dbo.PropertyDocuments;
IF OBJECT_ID(N'dbo.DocumentTypes', N'U') IS NOT NULL DROP TABLE dbo.DocumentTypes;
GO

CREATE TABLE dbo.DocumentTypes (
  Id                 INT IDENTITY(1,1) PRIMARY KEY,
  Code               NVARCHAR(30) NOT NULL,
  NameAr             NVARCHAR(100) COLLATE Arabic_CI_AS NOT NULL,
  NameEn             NVARCHAR(100) NOT NULL,
  Category           NVARCHAR(30) NOT NULL,
  Description        NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  IsRequired         BIT NOT NULL CONSTRAINT DF_DocTypes_IsRequired DEFAULT 0,
  HasExpiry          BIT NOT NULL CONSTRAINT DF_DocTypes_HasExpiry DEFAULT 0,
  AlertDays1         INT NULL CONSTRAINT DF_DocTypes_AlertDays1 DEFAULT 90,
  AlertDays2         INT NULL CONSTRAINT DF_DocTypes_AlertDays2 DEFAULT 30,
  AllowedExtensions  NVARCHAR(200) NOT NULL CONSTRAINT DF_DocTypes_AllowedExt DEFAULT N'pdf,jpg,jpeg,png,tiff',
  MaxFileSizeMB      INT NOT NULL CONSTRAINT DF_DocTypes_MaxSize DEFAULT 10,
  VerifierRoles      NVARCHAR(200) NULL,
  IsActive           BIT NOT NULL CONSTRAINT DF_DocTypes_IsActive DEFAULT 1,
  SortOrder          INT NOT NULL CONSTRAINT DF_DocTypes_SortOrder DEFAULT 0,
  CreatedAt          DATETIME2 NOT NULL CONSTRAINT DF_DocTypes_CreatedAt DEFAULT GETUTCDATE(),
  CreatedById        INT NULL,
  CONSTRAINT UQ_DocTypeCode UNIQUE (Code)
);
GO

CREATE TABLE dbo.PropertyDocuments (
  Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,
  PropertyId          INT NOT NULL,
  DocumentTypeId      INT NOT NULL,
  DocumentNumber      NVARCHAR(100) NULL,
  Title               NVARCHAR(300) COLLATE Arabic_CI_AS NOT NULL,
  Description         NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  IssuingAuthority    NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  IssueDate           DATE NULL,
  ExpiryDate          DATE NULL,
  Status              NVARCHAR(30) NOT NULL CONSTRAINT DF_PropDoc_Status DEFAULT N'PendingVerification',
  CurrentVersionId    BIGINT NULL,
  VersionCount        INT NOT NULL CONSTRAINT DF_PropDoc_VersionCount DEFAULT 0,
  LinkedUnitId        INT NULL,
  LinkedPartnershipId BIGINT NULL,
  PrimaryResponsibleId INT NULL,
  VerifiedById        INT NULL,
  VerifiedAt          DATETIME2 NULL,
  VerificationNotes   NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  RejectionReason     NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  OcrText             NVARCHAR(MAX) NULL,
  OcrConfidence       DECIMAL(5,2) NULL,
  OcrProcessedAt      DATETIME2 NULL,
  Alert1Sent          BIT NOT NULL CONSTRAINT DF_PropDoc_Alert1 DEFAULT 0,
  Alert2Sent          BIT NOT NULL CONSTRAINT DF_PropDoc_Alert2 DEFAULT 0,
  ExpiredAlertSent    BIT NOT NULL CONSTRAINT DF_PropDoc_AlertExpired DEFAULT 0,
  Tags                NVARCHAR(500) NULL,
  IsDeleted           BIT NOT NULL CONSTRAINT DF_PropDoc_IsDeleted DEFAULT 0,
  DeletedAt           DATETIME2 NULL,
  DeletedById         INT NULL,
  DeletedReason       NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  CreatedAt           DATETIME2 NOT NULL CONSTRAINT DF_PropDoc_CreatedAt DEFAULT GETUTCDATE(),
  CreatedById         INT NOT NULL,
  UpdatedAt           DATETIME2 NULL,
  CONSTRAINT FK_PropertyDocuments_Properties_PropertyId FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(Id),
  CONSTRAINT FK_PropertyDocuments_DocumentTypes_DocumentTypeId FOREIGN KEY (DocumentTypeId) REFERENCES dbo.DocumentTypes(Id),
  CONSTRAINT FK_PropertyDocuments_PropertyUnits_LinkedUnitId FOREIGN KEY (LinkedUnitId) REFERENCES dbo.PropertyUnits(Id),
  CONSTRAINT FK_PropertyDocuments_Users_PrimaryResponsibleId FOREIGN KEY (PrimaryResponsibleId) REFERENCES dbo.Users(Id),
  CONSTRAINT FK_PropertyDocuments_Users_VerifiedById FOREIGN KEY (VerifiedById) REFERENCES dbo.Users(Id),
  CONSTRAINT FK_PropertyDocuments_Users_DeletedById FOREIGN KEY (DeletedById) REFERENCES dbo.Users(Id),
  CONSTRAINT FK_PropertyDocuments_Users_CreatedById FOREIGN KEY (CreatedById) REFERENCES dbo.Users(Id),
  CONSTRAINT CK_DocStatus CHECK (Status IN (N'PendingVerification',N'Verified',N'Rejected',N'ExpiringSoon',N'Expired',N'Archived'))
);
GO

CREATE TABLE dbo.DocumentVersions (
  Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
  DocumentId      BIGINT NOT NULL,
  VersionNumber   INT NOT NULL,
  FileUrl         NVARCHAR(500) NOT NULL,
  FileName        NVARCHAR(200) NOT NULL,
  FileExtension   NVARCHAR(10) NOT NULL,
  FileSizeBytes   BIGINT NOT NULL,
  MimeType        NVARCHAR(100) NOT NULL,
  ThumbnailUrl    NVARCHAR(500) NULL,
  PageCount       INT NULL,
  UploadedById    INT NOT NULL,
  UploadedAt      DATETIME2 NOT NULL CONSTRAINT DF_DocVer_UploadedAt DEFAULT GETUTCDATE(),
  IsCurrent       BIT NOT NULL CONSTRAINT DF_DocVer_IsCurrent DEFAULT 1,
  Notes           NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  CONSTRAINT UQ_DocVersion UNIQUE (DocumentId, VersionNumber),
  CONSTRAINT FK_DocVersions_Documents_DocumentId FOREIGN KEY (DocumentId) REFERENCES dbo.PropertyDocuments(Id),
  CONSTRAINT FK_DocVersions_Users_UploadedById FOREIGN KEY (UploadedById) REFERENCES dbo.Users(Id)
);
GO

ALTER TABLE dbo.PropertyDocuments
ADD CONSTRAINT FK_PropertyDocuments_DocumentVersions_CurrentVersionId
FOREIGN KEY (CurrentVersionId) REFERENCES dbo.DocumentVersions(Id);
GO

CREATE TABLE dbo.DocumentAuditTrail (
  Id             BIGINT IDENTITY(1,1) PRIMARY KEY,
  DocumentId     BIGINT NOT NULL,
  PropertyId     INT NOT NULL,
  ActionType     NVARCHAR(50) NOT NULL,
  ActionByUserId INT NULL,
  ActionAt       DATETIME2 NOT NULL CONSTRAINT DF_DocAudit_ActionAt DEFAULT GETUTCDATE(),
  VersionId      BIGINT NULL,
  Details        NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  IpAddress      NVARCHAR(50) NULL,
  OldValue       NVARCHAR(500) NULL,
  NewValue       NVARCHAR(500) NULL,
  CONSTRAINT FK_DocAudit_Documents_DocumentId FOREIGN KEY (DocumentId) REFERENCES dbo.PropertyDocuments(Id),
  CONSTRAINT FK_DocAudit_Users_ActionByUserId FOREIGN KEY (ActionByUserId) REFERENCES dbo.Users(Id),
  CONSTRAINT FK_DocAudit_Versions_VersionId FOREIGN KEY (VersionId) REFERENCES dbo.DocumentVersions(Id)
);
GO

CREATE TABLE dbo.DocumentAlerts (
  Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
  DocumentId       BIGINT NOT NULL,
  PropertyId       INT NOT NULL,
  AlertLevel       TINYINT NOT NULL,
  AlertType        NVARCHAR(30) NOT NULL,
  DaysRemaining    INT NULL,
  IsRead           BIT NOT NULL CONSTRAINT DF_DocAlert_IsRead DEFAULT 0,
  ReadAt           DATETIME2 NULL,
  ReadByUserId     INT NULL,
  NotifiedUserIds  NVARCHAR(500) NULL,
  CreatedAt        DATETIME2 NOT NULL CONSTRAINT DF_DocAlert_CreatedAt DEFAULT GETUTCDATE(),
  CONSTRAINT FK_DocAlert_Documents_DocumentId FOREIGN KEY (DocumentId) REFERENCES dbo.PropertyDocuments(Id),
  CONSTRAINT FK_DocAlert_Users_ReadByUserId FOREIGN KEY (ReadByUserId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.DocumentResponsibles (
  Id           INT IDENTITY(1,1) PRIMARY KEY,
  DocumentId   BIGINT NOT NULL,
  UserId       INT NOT NULL,
  AssignedById INT NOT NULL,
  AssignedAt   DATETIME2 NOT NULL CONSTRAINT DF_DocResp_AssignedAt DEFAULT GETUTCDATE(),
  IsActive     BIT NOT NULL CONSTRAINT DF_DocResp_IsActive DEFAULT 1,
  Notes        NVARCHAR(300) COLLATE Arabic_CI_AS NULL,
  CONSTRAINT UQ_DocResponsible UNIQUE (DocumentId, UserId),
  CONSTRAINT FK_DocResp_Documents_DocumentId FOREIGN KEY (DocumentId) REFERENCES dbo.PropertyDocuments(Id),
  CONSTRAINT FK_DocResp_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
  CONSTRAINT FK_DocResp_Users_AssignedById FOREIGN KEY (AssignedById) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_PropDoc_Property ON dbo.PropertyDocuments(PropertyId, IsDeleted, Status);
CREATE INDEX IX_PropDoc_Type ON dbo.PropertyDocuments(DocumentTypeId, Status);
CREATE INDEX IX_PropDoc_Expiry ON dbo.PropertyDocuments(ExpiryDate, Status, IsDeleted) WHERE ExpiryDate IS NOT NULL;
CREATE INDEX IX_PropDoc_Responsible ON dbo.PropertyDocuments(PrimaryResponsibleId, Status);
CREATE INDEX IX_DocVersion_Doc ON dbo.DocumentVersions(DocumentId, IsCurrent);
CREATE INDEX IX_DocAudit_Doc ON dbo.DocumentAuditTrail(DocumentId, ActionAt DESC);
CREATE INDEX IX_DocAudit_Property ON dbo.DocumentAuditTrail(PropertyId, ActionAt DESC);
CREATE INDEX IX_DocAlert_Doc ON dbo.DocumentAlerts(DocumentId, IsRead);
CREATE INDEX IX_DocAlert_Level ON dbo.DocumentAlerts(AlertLevel, IsRead, CreatedAt);
CREATE INDEX IX_DocResponsible_User ON dbo.DocumentResponsibles(UserId, IsActive);
GO

IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'WaqfFullTextCatalog')
    CREATE FULLTEXT CATALOG WaqfFullTextCatalog AS DEFAULT;

  IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.PropertyDocuments'))
    CREATE FULLTEXT INDEX ON dbo.PropertyDocuments
    (
      OcrText LANGUAGE 1025,
      Title LANGUAGE 1025,
      DocumentNumber LANGUAGE 1025,
      Tags LANGUAGE 1025
    )
    KEY INDEX PK__PropertyDocuments__3214EC07
    WITH CHANGE_TRACKING AUTO;
END;
GO

INSERT INTO dbo.DocumentTypes (Code,NameAr,NameEn,Category,IsRequired,HasExpiry,AlertDays1,AlertDays2,VerifierRoles,SortOrder)
VALUES
(N'Ownership',      N'صك الملكية / حجة الوقف', N'Ownership Deed',         N'Ownership',    1,0,NULL,NULL,N'["LEGAL_REVIEWER"]',1),
(N'Survey',         N'وثيقة الكاداسترو / الطابو', N'Cadastral Document', N'Survey',       1,0,NULL,NULL,N'["LEGAL_REVIEWER"]',2),
(N'BuildingPermit', N'رخصة البناء', N'Building Permit',                  N'Construction', 0,1,90, 30, N'["ENGINEER"]',3),
(N'Completion',     N'شهادة الإنجاز', N'Completion Certificate',         N'Construction', 0,0,NULL,NULL,N'["ENGINEER"]',4),
(N'CourtOrder',     N'حكم قضائي / قرار إداري', N'Court Order',          N'Legal',        0,0,NULL,NULL,N'["LEGAL_REVIEWER"]',5),
(N'Partnership',    N'وثيقة الشراكة', N'Partnership Agreement',          N'Partnership',  0,1,90, 30, N'["LEGAL_REVIEWER","CONTRACTS_MGR"]',6),
(N'LeaseContract',  N'عقد الإيجار', N'Lease Contract',                   N'Lease',        0,1,90, 30, N'["CONTRACTS_MGR"]',7),
(N'Engineering',    N'مخطط هندسي / صورة', N'Engineering Drawing',       N'Engineering',  0,0,NULL,NULL,N'["ENGINEER"]',8),
(N'Insurance',      N'وثيقة التأمين', N'Insurance Policy',               N'Insurance',    0,1,60, 30, N'["ANALYST"]',9),
(N'Valuation',      N'شهادة التقييم', N'Valuation Certificate',          N'Insurance',    0,1,365,60, N'["ANALYST"]',10);
GO
