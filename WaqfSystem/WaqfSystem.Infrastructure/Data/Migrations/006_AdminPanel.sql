SET NOCOUNT ON;

IF COL_LENGTH('Roles', 'DisplayNameAr') IS NULL
BEGIN
    ALTER TABLE Roles ADD
      DisplayNameAr NVARCHAR(100) COLLATE Arabic_CI_AS NULL,
      DisplayNameEn NVARCHAR(100) NULL,
      Description2 NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
      IsSystemRole BIT NOT NULL CONSTRAINT DF_Roles_IsSystemRole DEFAULT 0,
      Color NVARCHAR(20) NULL,
      Icon NVARCHAR(50) NULL,
      CreatedById INT NULL,
      UpdatedAt2 DATETIME2 NULL;
END;

IF COL_LENGTH('Roles', 'IsActive') IS NULL
BEGIN
    ALTER TABLE Roles ADD IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1;
END;

IF COL_LENGTH('Roles', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE Roles ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT GETUTCDATE();
END;

IF COL_LENGTH('Users', 'FullNameEn') IS NULL
BEGIN
    ALTER TABLE Users ADD
      FullNameEn NVARCHAR(200) NULL,
      NationalId NVARCHAR(20) NULL,
      Phone NVARCHAR(20) NULL,
      Phone2 NVARCHAR(20) NULL,
      JobTitle NVARCHAR(100) COLLATE Arabic_CI_AS NULL,
      GovernorateId INT NULL,
      TeamId INT NULL,
      IsLocked BIT NOT NULL CONSTRAINT DF_Users_IsLocked DEFAULT 0,
      LockReason NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
      LockedAt DATETIME2 NULL,
      LockedById INT NULL,
      LastLoginIp NVARCHAR(50) NULL,
      FailedLoginCount INT NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT 0,
      PasswordChangedAt DATETIME2 NULL,
      MustChangePassword BIT NOT NULL CONSTRAINT DF_Users_MustChangePassword DEFAULT 0,
      Notes NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
      CreatedById2 INT NULL,
      UpdatedAt2 DATETIME2 NULL;
END;

IF COL_LENGTH('Users', 'IsActive') IS NULL
BEGIN
    ALTER TABLE Users ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1;
END;

IF COL_LENGTH('Users', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE Users ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETUTCDATE();
END;

IF COL_LENGTH('Users', 'LastLoginAt') IS NULL
BEGIN
    ALTER TABLE Users ADD LastLoginAt DATETIME2 NULL;
END;

IF OBJECT_ID('Permissions', 'U') IS NULL
BEGIN
    CREATE TABLE Permissions (
      Id INT IDENTITY(1,1) PRIMARY KEY,
      PermissionKey NVARCHAR(100) NOT NULL,
      Module NVARCHAR(50) NOT NULL,
      [Action] NVARCHAR(50) NOT NULL,
      DisplayNameAr NVARCHAR(200) COLLATE Arabic_CI_AS NOT NULL,
      DisplayNameEn NVARCHAR(200) NOT NULL,
      Description NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
      IsActive BIT NOT NULL DEFAULT 1,
      CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      CONSTRAINT UQ_PermissionKey UNIQUE (PermissionKey)
    );
END;

IF OBJECT_ID('RolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE RolePermissions (
      RoleId INT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
      PermissionId INT NOT NULL REFERENCES Permissions(Id) ON DELETE CASCADE,
      GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
      GrantedById INT NOT NULL,
      PRIMARY KEY (RoleId, PermissionId)
    );
END;

IF COL_LENGTH('Governorates', 'PostalCode') IS NULL
BEGIN
    ALTER TABLE Governorates ADD
      PostalCode NVARCHAR(20) NULL,
      SortOrder INT NOT NULL CONSTRAINT DF_Gov_SortOrder DEFAULT 0,
      CreatedById INT NULL;
END;

IF COL_LENGTH('Districts', 'SortOrder') IS NULL
BEGIN
    ALTER TABLE Districts ADD
      SortOrder INT NOT NULL CONSTRAINT DF_District_SortOrder DEFAULT 0,
      CreatedById INT NULL;
END;

IF COL_LENGTH('SubDistricts', 'SortOrder') IS NULL
BEGIN
    ALTER TABLE SubDistricts ADD
      SortOrder INT NOT NULL CONSTRAINT DF_SubDistrict_SortOrder DEFAULT 0,
      CreatedById INT NULL;
END;

IF COL_LENGTH('Neighborhoods', 'Type') IS NULL
BEGIN
    ALTER TABLE Neighborhoods ADD
      [Type] NVARCHAR(20) NOT NULL CONSTRAINT DF_Neighborhood_Type DEFAULT 'City',
      PostalCode NVARCHAR(20) NULL,
      SortOrder INT NOT NULL CONSTRAINT DF_Neighborhood_SortOrder DEFAULT 0,
      CreatedById INT NULL;

    ALTER TABLE Neighborhoods WITH CHECK ADD CONSTRAINT CK_NeighType CHECK ([Type] IN ('City','Village','Camp','Informal','Industrial'));
END;

IF COL_LENGTH('Streets', 'GisLineId') IS NULL
BEGIN
    ALTER TABLE Streets ADD
      GisLineId NVARCHAR(100) NULL,
      StreetType NVARCHAR(30) NULL,
      SortOrder INT NOT NULL CONSTRAINT DF_Street_SortOrder DEFAULT 0,
      CreatedById INT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Permissions_Module' AND object_id = OBJECT_ID('Permissions'))
    CREATE INDEX IX_Permissions_Module ON Permissions(Module, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RolePerms_Role' AND object_id = OBJECT_ID('RolePermissions'))
    CREATE INDEX IX_RolePerms_Role ON RolePermissions(RoleId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Role' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX IX_Users_Role ON Users(RoleId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Gov' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX IX_Users_Gov ON Users(GovernorateId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Gov_Active' AND object_id = OBJECT_ID('Governorates'))
    CREATE INDEX IX_Gov_Active ON Governorates(IsActive, SortOrder);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_District_Gov' AND object_id = OBJECT_ID('Districts'))
    CREATE INDEX IX_District_Gov ON Districts(GovernorateId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SubDist_Dist' AND object_id = OBJECT_ID('SubDistricts'))
    CREATE INDEX IX_SubDist_Dist ON SubDistricts(DistrictId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Neigh_Sub' AND object_id = OBJECT_ID('Neighborhoods'))
    CREATE INDEX IX_Neigh_Sub ON Neighborhoods(SubDistrictId, IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Street_Neigh' AND object_id = OBJECT_ID('Streets'))
    CREATE INDEX IX_Street_Neigh ON Streets(NeighborhoodId, IsActive);

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'SYS_ADMIN')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مدير النظام', N'System Admin', 'SYS_ADMIN', N'مدير النظام', 'System Admin', 1, 1, '#DC2626', 'bi-shield-fill-check');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'AUTH_DIRECTOR')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مدير الهيئة', N'Authority Director', 'AUTH_DIRECTOR', N'مدير الهيئة', 'Authority Director', 1, 1, '#7C3AED', 'bi-building');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'REGIONAL_MGR')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مدير إقليمي', N'Regional Manager', 'REGIONAL_MGR', N'مدير إقليمي', 'Regional Manager', 1, 1, '#1E40AF', 'bi-geo-alt-fill');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'LEGAL_REVIEWER')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مدقق قانوني', N'Legal Reviewer', 'LEGAL_REVIEWER', N'مدقق قانوني', 'Legal Reviewer', 1, 1, '#92400E', 'bi-file-earmark-text');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'ENGINEER')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مهندس مُقيِّم', N'Engineer', 'ENGINEER', N'مهندس مُقيِّم', 'Engineer', 1, 1, '#065F46', 'bi-tools');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'FIELD_SUPERVISOR')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مشرف ميداني', N'Field Supervisor', 'FIELD_SUPERVISOR', N'مشرف ميداني', 'Field Supervisor', 1, 1, '#1E40AF', 'bi-person-badge');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'FIELD_INSPECTOR')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مفتش ميداني', N'Field Inspector', 'FIELD_INSPECTOR', N'مفتش ميداني', 'Field Inspector', 1, 1, '#1D4ED8', 'bi-clipboard-check');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'COLLECTOR')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'محصِّل إيرادات', N'Collector', 'COLLECTOR', N'محصِّل إيرادات', 'Collector', 1, 1, '#6B21A8', 'bi-cash-coin');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'CONTRACTS_MGR')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'مدير عقود', N'Contracts Manager', 'CONTRACTS_MGR', N'مدير عقود', 'Contracts Manager', 1, 1, '#0E7490', 'bi-file-earmark-ruled');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Code = 'ANALYST')
INSERT INTO Roles (NameAr, NameEn, Code, DisplayNameAr, DisplayNameEn, IsSystemRole, IsActive, Color, Icon)
VALUES
  (N'محلل مالي', N'Analyst', 'ANALYST', N'محلل مالي', 'Analyst', 1, 1, '#4338CA', 'bi-graph-up');

IF NOT EXISTS (SELECT 1 FROM Governorates WHERE Code = 'BGH')
BEGIN
    INSERT INTO Governorates (CountryId, NameAr, NameEn, Code, IsActive, SortOrder, CreatedById)
    VALUES
      (1, N'بغداد', 'Baghdad', 'BGH', 1, 1, 1),
      (1, N'نينوى', 'Nineveh', 'NNW', 1, 2, 1),
      (1, N'البصرة', 'Basra', 'BSR', 1, 3, 1),
      (1, N'الأنبار', 'Anbar', 'ANB', 1, 4, 1),
      (1, N'صلاح الدين', 'Saladin', 'SLD', 1, 5, 1),
      (1, N'ديالى', 'Diyala', 'DYL', 1, 6, 1),
      (1, N'كركوك', 'Kirkuk', 'KRK', 1, 7, 1),
      (1, N'واسط', 'Wasit', 'WST', 1, 8, 1),
      (1, N'بابل', 'Babylon', 'BBL', 1, 9, 1),
      (1, N'كربلاء', 'Karbala', 'KBL', 1, 10, 1),
      (1, N'النجف', 'Najaf', 'NJF', 1, 11, 1),
      (1, N'القادسية', 'Qadisiyah', 'QDS', 1, 12, 1),
      (1, N'المثنى', 'Muthanna', 'MTN', 1, 13, 1),
      (1, N'ذي قار', 'Dhi Qar', 'DHQ', 1, 14, 1),
      (1, N'ميسان', 'Maysan', 'MSN', 1, 15, 1),
      (1, N'دهوك', 'Duhok', 'DHK', 1, 16, 1),
      (1, N'أربيل', 'Erbil', 'EBL', 1, 17, 1),
      (1, N'السليمانية', 'Sulaymaniyah', 'SLM', 1, 18, 1);
END;