-- =============================================
-- Waqf Property Census System — نظام حصر الأملاك
-- Iraqi Sunni Waqf Authority
-- Database: WaqfSystem
-- Collation: Arabic_CI_AS
-- =============================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'WaqfSystem')
BEGIN
    CREATE DATABASE [WaqfSystem]
    COLLATE Arabic_CI_AS;
END
GO

USE [WaqfSystem];
GO

-- =============================================
-- ROLES
-- =============================================
CREATE TABLE [dbo].[Roles] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [NameAr]        NVARCHAR(100)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(100)   NOT NULL,
    [Code]          NVARCHAR(50)    NOT NULL,
    [Description]   NVARCHAR(500)   COLLATE Arabic_CI_AS NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Roles_Code] UNIQUE ([Code])
);
GO

-- =============================================
-- USERS
-- =============================================
CREATE TABLE [dbo].[Users] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [FullNameAr]            NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [FullNameEn]            NVARCHAR(200)   NULL,
    [Email]                 NVARCHAR(256)   NOT NULL,
    [PasswordHash]          NVARCHAR(500)   NOT NULL,
    [PhoneNumber]           NVARCHAR(20)    NULL,
    [NationalId]            NVARCHAR(20)    NULL,
    [RoleId]                INT             NOT NULL,
    [GovernorateId]         INT             NULL,
    [TeamId]                INT             NULL,
    [IsActive]              BIT             NOT NULL DEFAULT 1,
    [LastLoginAt]           DATETIME2       NULL,
    [RefreshToken]          NVARCHAR(500)   NULL,
    [RefreshTokenExpiresAt] DATETIME2       NULL,
    [DeviceId]              NVARCHAR(200)   NULL,
    [ProfilePhotoUrl]       NVARCHAR(500)   NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Users_Email] UNIQUE ([Email]),
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]),
    CONSTRAINT [FK_Users_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- GEOGRAPHIC HIERARCHY (7 levels)
-- =============================================
CREATE TABLE [dbo].[Countries] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [NameAr]        NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(200)   NULL,
    [Code]          NVARCHAR(10)    NOT NULL,
    [GisLayerId]    NVARCHAR(100)   NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Countries_Code] UNIQUE ([Code])
);
GO

CREATE TABLE [dbo].[Governorates] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [CountryId]     INT             NOT NULL,
    [NameAr]        NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(200)   NULL,
    [Code]          NVARCHAR(10)    NOT NULL,
    [GisLayerId]    NVARCHAR(100)   NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_Governorates] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Governorates_Code] UNIQUE ([Code]),
    CONSTRAINT [FK_Governorates_Countries] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Countries]([Id])
);
GO

CREATE TABLE [dbo].[Districts] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [GovernorateId] INT             NOT NULL,
    [NameAr]        NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(200)   NULL,
    [Code]          NVARCHAR(10)    NOT NULL,
    [GisLayerId]    NVARCHAR(100)   NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_Districts] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Districts_Governorates] FOREIGN KEY ([GovernorateId]) REFERENCES [dbo].[Governorates]([Id])
);
GO

CREATE TABLE [dbo].[SubDistricts] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [DistrictId]    INT             NOT NULL,
    [NameAr]        NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(200)   NULL,
    [Code]          NVARCHAR(10)    NOT NULL,
    [GisLayerId]    NVARCHAR(100)   NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_SubDistricts] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_SubDistricts_Districts] FOREIGN KEY ([DistrictId]) REFERENCES [dbo].[Districts]([Id])
);
GO

CREATE TABLE [dbo].[Neighborhoods] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [SubDistrictId] INT             NOT NULL,
    [NameAr]        NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(200)   NULL,
    [Code]          NVARCHAR(20)    NOT NULL,
    [GisLayerId]    NVARCHAR(100)   NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_Neighborhoods] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Neighborhoods_SubDistricts] FOREIGN KEY ([SubDistrictId]) REFERENCES [dbo].[SubDistricts]([Id])
);
GO

CREATE TABLE [dbo].[Streets] (
    [Id]            INT             IDENTITY(1,1) NOT NULL,
    [NeighborhoodId] INT            NOT NULL,
    [NameAr]        NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [NameEn]        NVARCHAR(200)   NULL,
    [Code]          NVARCHAR(20)    NOT NULL,
    [GisLayerId]    NVARCHAR(100)   NULL,
    [IsActive]      BIT             NOT NULL DEFAULT 1,
    [IsDeleted]     BIT             NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2       NULL,
    CONSTRAINT [PK_Streets] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Streets_Neighborhoods] FOREIGN KEY ([NeighborhoodId]) REFERENCES [dbo].[Neighborhoods]([Id])
);
GO

-- =============================================
-- PROPERTIES (main entity)
-- =============================================
CREATE TABLE [dbo].[Properties] (
    [Id]                        INT             IDENTITY(1,1) NOT NULL,
    [WqfNumber]                 NVARCHAR(50)    NOT NULL,
    [PropertyName]              NVARCHAR(300)   COLLATE Arabic_CI_AS NULL,
    [PropertyNameEn]            NVARCHAR(300)   NULL,
    [PropertyType]              TINYINT         NOT NULL,          -- enum PropertyType
    [PropertyCategory]          TINYINT         NOT NULL DEFAULT 0,-- enum PropertyCategory (Building/Agricultural/Land)
    [WaqfType]                  TINYINT         NULL,              -- enum WaqfType (Khairi/Dhurri/Mushtarak)
    [OwnershipType]             TINYINT         NOT NULL DEFAULT 0,-- enum OwnershipType (FULL_WAQF/PARTNERSHIP)
    [OwnershipPercentage]       DECIMAL(5,2)    NOT NULL DEFAULT 100.00,
    [DeedNumber]                NVARCHAR(50)    NULL,
    [CadastralNumber]           NVARCHAR(50)    NULL,
    [TabuNumber]                NVARCHAR(50)    NULL,
    [RegistrationDate]          DATE            NULL,
    [WaqfOriginStory]           NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [FounderName]               NVARCHAR(200)   COLLATE Arabic_CI_AS NULL,
    [FoundationDate]            DATE            NULL,
    [EndowmentPurpose]          NVARCHAR(500)   COLLATE Arabic_CI_AS NULL,

    -- Building info
    [TotalFloors]               SMALLINT        NULL,
    [BasementFloors]            SMALLINT        NULL DEFAULT 0,
    [TotalUnits]                INT             NULL,
    [TotalAreaSqm]              DECIMAL(12,2)   NULL,
    [BuiltUpAreaSqm]            DECIMAL(12,2)   NULL,
    [LandAreaSqm]               DECIMAL(12,2)   NULL,
    [YearBuilt]                 SMALLINT        NULL,
    [LastRenovationYear]        SMALLINT        NULL,
    [ConstructionType]          TINYINT         NULL,              -- enum ConstructionType
    [StructuralCondition]       TINYINT         NULL,              -- enum StructuralCondition
    [FacadeType]                NVARCHAR(100)   COLLATE Arabic_CI_AS NULL,
    [RoofType]                  NVARCHAR(100)   COLLATE Arabic_CI_AS NULL,

    -- Financial
    [EstimatedValue]            DECIMAL(18,2)   NULL,
    [LastValuationDate]         DATE            NULL,
    [AnnualRevenue]             DECIMAL(18,2)   NULL,
    [AnnualExpenses]            DECIMAL(18,2)   NULL,
    [InsuranceValue]            DECIMAL(18,2)   NULL,
    [InsuranceExpiryDate]       DATE            NULL,

    -- Status & Workflow
    [PropertyStatus]            TINYINT         NOT NULL DEFAULT 0,-- enum PropertyStatus
    [ApprovalStage]             TINYINT         NOT NULL DEFAULT 0,-- enum ApprovalStage
    [DqsScore]                  DECIMAL(5,2)    NOT NULL DEFAULT 0,

    -- GIS
    [GisFeatureId]              NVARCHAR(100)   NULL,
    [GisPolygonId]              NVARCHAR(100)   NULL,
    [GisLayerName]              NVARCHAR(100)   NULL,
    [GisPolygon]                NVARCHAR(MAX)   NULL,              -- GeoJSON
    [Latitude]                  DECIMAL(10,7)   NULL,
    [Longitude]                 DECIMAL(10,7)   NULL,
    [GpsAccuracyMeters]         DECIMAL(6,2)    NULL,
    [SatelliteImageUrl]         NVARCHAR(500)   NULL,
    [LastGisSyncAt]             DATETIME2       NULL,
    [GisSyncStatus]             TINYINT         NOT NULL DEFAULT 0,-- enum GisSyncStatus

    -- Audit
    [Notes]                     NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [LocalId]                   NVARCHAR(100)   NULL,              -- device UUID for offline sync
    [DeviceId]                  NVARCHAR(200)   NULL,
    [IsDeleted]                 BIT             NOT NULL DEFAULT 0,
    [CreatedAt]                 DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]                 DATETIME2       NULL,
    [CreatedById]               INT             NOT NULL,
    [UpdatedById]               INT             NULL,
    [GovernorateId]             INT             NULL,

    CONSTRAINT [PK_Properties] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Properties_WqfNumber] UNIQUE ([WqfNumber]),
    CONSTRAINT [FK_Properties_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Properties_UpdatedBy] FOREIGN KEY ([UpdatedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Properties_Governorate] FOREIGN KEY ([GovernorateId]) REFERENCES [dbo].[Governorates]([Id]),
    CONSTRAINT [CK_Properties_OwnershipPct] CHECK ([OwnershipPercentage] > 0 AND [OwnershipPercentage] <= 100.00)
);
GO

-- =============================================
-- PROPERTY ADDRESSES
-- =============================================
CREATE TABLE [dbo].[PropertyAddresses] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [StreetId]              INT             NULL,
    [BuildingNumber]        NVARCHAR(20)    NULL,
    [PlotNumber]            NVARCHAR(20)    NULL,
    [BlockNumber]           NVARCHAR(20)    NULL,
    [ZoneNumber]            NVARCHAR(20)    NULL,
    [NearestLandmark]       NVARCHAR(300)   COLLATE Arabic_CI_AS NULL,
    [AlternativeAddress]    NVARCHAR(500)   COLLATE Arabic_CI_AS NULL,
    [What3Words]            NVARCHAR(100)   NULL,
    [PlusCodes]             NVARCHAR(100)   NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyAddresses] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyAddresses_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyAddresses_Streets] FOREIGN KEY ([StreetId]) REFERENCES [dbo].[Streets]([Id]),
    CONSTRAINT [FK_PropertyAddresses_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY FLOORS
-- =============================================
CREATE TABLE [dbo].[PropertyFloors] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [FloorNumber]           SMALLINT        NOT NULL,
    [FloorLabel]            NVARCHAR(50)    COLLATE Arabic_CI_AS NULL,
    [FloorUsage]            TINYINT         NOT NULL DEFAULT 0,    -- enum FloorUsage
    [TotalAreaSqm]          DECIMAL(10,2)   NULL,
    [UsableAreaSqm]         DECIMAL(10,2)   NULL,
    [CeilingHeightCm]       SMALLINT        NULL,
    [StructuralCondition]   TINYINT         NULL,                  -- enum StructuralCondition
    [HasBalcony]            BIT             NOT NULL DEFAULT 0,
    [FloorPlanUrl]          NVARCHAR(500)   NULL,
    [IsOccupied]            BIT             NOT NULL DEFAULT 0,
    [Notes]                 NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyFloors] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyFloors_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyFloors_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [CK_PropertyFloors_FloorNumber] CHECK ([FloorNumber] >= -3 AND [FloorNumber] <= 100)
);
GO

-- =============================================
-- PROPERTY UNITS
-- =============================================
CREATE TABLE [dbo].[PropertyUnits] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [FloorId]               INT             NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [UnitNumber]            NVARCHAR(20)    COLLATE Arabic_CI_AS NULL,
    [UnitType]              TINYINT         NOT NULL DEFAULT 0,    -- enum UnitType
    [AreaSqm]               DECIMAL(10,2)   NULL,
    [BedroomCount]          SMALLINT        NULL DEFAULT 0,
    [BathroomCount]         SMALLINT        NULL DEFAULT 0,
    [OccupancyStatus]       TINYINT         NOT NULL DEFAULT 0,    -- enum OccupancyStatus
    [CurrentContractId]     INT             NULL,
    [MarketRentMonthly]     DECIMAL(12,2)   NULL,
    [ElectricMeterNo]       NVARCHAR(50)    NULL,
    [WaterMeterNo]          NVARCHAR(50)    NULL,
    [HasAC]                 BIT             NOT NULL DEFAULT 0,
    [HasKitchen]            BIT             NOT NULL DEFAULT 0,
    [Furnished]             TINYINT         NOT NULL DEFAULT 1,    -- enum FurnishedStatus
    [Condition]             TINYINT         NULL,                  -- enum StructuralCondition
    [PhotoUrls]             NVARCHAR(MAX)   NULL,                  -- JSON array
    [UnitFloorPlanUrl]      NVARCHAR(500)   NULL,
    [Notes]                 NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyUnits] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyUnits_Floors] FOREIGN KEY ([FloorId]) REFERENCES [dbo].[PropertyFloors]([Id]),
    CONSTRAINT [FK_PropertyUnits_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyUnits_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY ROOMS
-- =============================================
CREATE TABLE [dbo].[PropertyRooms] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [UnitId]                INT             NOT NULL,
    [RoomType]              TINYINT         NOT NULL DEFAULT 0,    -- enum RoomType
    [AreaSqm]               DECIMAL(8,2)    NULL,
    [Length]                 DECIMAL(6,2)    NULL,
    [Width]                 DECIMAL(6,2)    NULL,
    [WindowsCount]          SMALLINT        NULL DEFAULT 0,
    [Condition]             TINYINT         NULL,                  -- enum StructuralCondition
    [Notes]                 NVARCHAR(500)   COLLATE Arabic_CI_AS NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyRooms] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyRooms_Units] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[PropertyUnits]([Id]),
    CONSTRAINT [FK_PropertyRooms_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY FACILITIES
-- =============================================
CREATE TABLE [dbo].[PropertyFacilities] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [FacilityType]          TINYINT         NOT NULL DEFAULT 0,    -- enum FacilityType
    [Details]               NVARCHAR(500)   COLLATE Arabic_CI_AS NULL,
    [Capacity]              INT             NULL,
    [LastMaintenanceDate]   DATE            NULL,
    [IsOperational]         BIT             NOT NULL DEFAULT 1,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyFacilities] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyFacilities_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyFacilities_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY METERS
-- =============================================
CREATE TABLE [dbo].[PropertyMeters] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [UnitId]                INT             NOT NULL,
    [MeterType]             TINYINT         NOT NULL DEFAULT 0,    -- enum MeterType (Electric/Water/Gas)
    [MeterNumber]           NVARCHAR(50)    NULL,
    [SubscriberNumber]      NVARCHAR(50)    NULL,
    [IssuingAuthority]      NVARCHAR(200)   COLLATE Arabic_CI_AS NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyMeters] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyMeters_Units] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[PropertyUnits]([Id]),
    CONSTRAINT [FK_PropertyMeters_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY PARTNERSHIPS
-- =============================================
CREATE TABLE [dbo].[PropertyPartnerships] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [PartnerName]           NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [PartnerType]           TINYINT         NOT NULL DEFAULT 0,    -- enum PartnerType (Individual/Company/Heirs/Government)
    [PartnerNationalId]     NVARCHAR(20)    NULL,
    [PartnerSharePercent]   DECIMAL(5,2)    NOT NULL,
    [PartnerBankIBAN]       NVARCHAR(34)    NULL,
    [RevenueDistribMethod]  TINYINT         NOT NULL DEFAULT 0,    -- enum RevenueDistribMethod
    [AgreementDocUrl]       NVARCHAR(500)   NULL,
    [AgreementDate]         DATE            NULL,
    [IsActive]              BIT             NOT NULL DEFAULT 1,
    [ContactPhone]          NVARCHAR(20)    NULL,
    [ContactEmail]          NVARCHAR(256)   NULL,
    [Notes]                 NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyPartnerships] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyPartnerships_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyPartnerships_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [CK_PropertyPartnerships_SharePct] CHECK ([PartnerSharePercent] > 0 AND [PartnerSharePercent] < 100.00)
);
GO

-- =============================================
-- AGRICULTURAL DETAILS
-- =============================================
CREATE TABLE [dbo].[AgriculturalDetails] (
    [Id]                        INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]                INT             NOT NULL,
    [TotalAreaDunum]            DECIMAL(12,2)   NULL,
    [CultivatedAreaDunum]       DECIMAL(12,2)   NULL,
    [UncultivatedAreaDunum]     DECIMAL(12,2)   NULL,
    [SoilType]                  TINYINT         NULL,              -- enum SoilType
    [SoilFertilityRating]       TINYINT         NULL,              -- 1-5
    [WaterSourceType]           TINYINT         NULL,              -- enum WaterSourceType
    [WaterRightsDocUrl]         NVARCHAR(500)   NULL,
    [IrrigationMethod]          TINYINT         NULL,              -- enum IrrigationMethod
    [WaterAvailabilityMonths]   NVARCHAR(MAX)   NULL,              -- JSON array of months
    [PrimaryHarvestType]        NVARCHAR(100)   COLLATE Arabic_CI_AS NULL,
    [SecondaryHarvestType]      NVARCHAR(100)   COLLATE Arabic_CI_AS NULL,
    [SeasonType]                TINYINT         NULL,              -- enum SeasonType
    [AverageYieldTonPerDunum]   DECIMAL(8,2)    NULL,
    [FarmerName]                NVARCHAR(200)   COLLATE Arabic_CI_AS NULL,
    [FarmerNationalId]          NVARCHAR(20)    NULL,
    [FarmingContractType]       TINYINT         NULL,              -- enum FarmingContractType
    [WaqfShareOfHarvest]        DECIMAL(5,2)    NULL,
    [FarmingStartDate]          DATE            NULL,
    [FarmingEndDate]            DATE            NULL,
    [HasFarmBuilding]           BIT             NOT NULL DEFAULT 0,
    [HasStorage]                BIT             NOT NULL DEFAULT 0,
    [HasWell]                   BIT             NOT NULL DEFAULT 0,
    [HasRoadAccess]             BIT             NOT NULL DEFAULT 0,
    [AnnualRevenueEstimate]     DECIMAL(18,2)   NULL,
    [LandValuePerDunum]         DECIMAL(18,2)   NULL,
    [LastInspectionDate]        DATE            NULL,
    [LastHarvestYear]           SMALLINT        NULL,
    [IsDeleted]                 BIT             NOT NULL DEFAULT 0,
    [CreatedAt]                 DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]                 DATETIME2       NULL,
    [CreatedById]               INT             NOT NULL,
    CONSTRAINT [PK_AgriculturalDetails] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_AgriculturalDetails_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_AgriculturalDetails_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [CK_AgriculturalDetails_SoilFertility] CHECK ([SoilFertilityRating] IS NULL OR ([SoilFertilityRating] >= 1 AND [SoilFertilityRating] <= 5)),
    CONSTRAINT [CK_AgriculturalDetails_CultivatedArea] CHECK ([CultivatedAreaDunum] IS NULL OR [TotalAreaDunum] IS NULL OR [CultivatedAreaDunum] <= [TotalAreaDunum])
);
GO

-- =============================================
-- PROPERTY DOCUMENTS
-- =============================================
CREATE TABLE [dbo].[PropertyDocuments] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [DocumentCategory]      TINYINT         NOT NULL DEFAULT 0,    -- enum DocumentCategory
    [DocumentType]          NVARCHAR(100)   COLLATE Arabic_CI_AS NULL,
    [DocumentNumber]        NVARCHAR(50)    NULL,
    [DocumentDate]          DATE            NULL,
    [ExpiryDate]            DATE            NULL,
    [IssuingAuthority]      NVARCHAR(200)   COLLATE Arabic_CI_AS NULL,
    [IssuingCity]           NVARCHAR(100)   COLLATE Arabic_CI_AS NULL,
    [FileUrl]               NVARCHAR(500)   NOT NULL,
    [FileFormat]            TINYINT         NOT NULL DEFAULT 0,    -- enum FileFormat
    [FileSizeKB]            INT             NULL,
    [IsOriginal]            BIT             NOT NULL DEFAULT 0,
    [IsVerified]            BIT             NOT NULL DEFAULT 0,
    [VerificationMethod]    TINYINT         NULL,                  -- enum VerificationMethod
    [VerifiedById]          INT             NULL,
    [VerifiedAt]            DATETIME2       NULL,
    [OcrText]               NVARCHAR(MAX)   NULL,
    [OcrConfidence]         DECIMAL(5,2)    NULL,
    [GisAttachedLayerId]    NVARCHAR(100)   NULL,
    [Notes]                 NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_PropertyDocuments] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyDocuments_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyDocuments_VerifiedBy] FOREIGN KEY ([VerifiedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_PropertyDocuments_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY PHOTOS
-- =============================================
CREATE TABLE [dbo].[PropertyPhotos] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [UnitId]                INT             NULL,
    [PhotoType]             TINYINT         NOT NULL DEFAULT 0,    -- enum PhotoType
    [FileUrl]               NVARCHAR(500)   NOT NULL,
    [ThumbnailUrl]          NVARCHAR(500)   NULL,
    [FileSizeKB]            INT             NULL,
    [Latitude]              DECIMAL(10,7)   NULL,
    [Longitude]             DECIMAL(10,7)   NULL,
    [TakenAt]               DATETIME2       NULL,
    [DeviceAccuracy]        DECIMAL(6,2)    NULL,
    [IsMain]                BIT             NOT NULL DEFAULT 0,
    [Caption]               NVARCHAR(300)   COLLATE Arabic_CI_AS NULL,
    [UploadedById]          INT             NOT NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    CONSTRAINT [PK_PropertyPhotos] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyPhotos_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyPhotos_Units] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[PropertyUnits]([Id]),
    CONSTRAINT [FK_PropertyPhotos_UploadedBy] FOREIGN KEY ([UploadedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PROPERTY WORKFLOW HISTORY
-- =============================================
CREATE TABLE [dbo].[PropertyWorkflowHistory] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [FromStage]             TINYINT         NOT NULL,
    [ToStage]               TINYINT         NOT NULL,
    [ActionById]            INT             NOT NULL,
    [ActionAt]              DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [Notes]                 NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [DqsAtAction]           DECIMAL(5,2)    NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    CONSTRAINT [PK_PropertyWorkflowHistory] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_PropertyWorkflowHistory_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id]),
    CONSTRAINT [FK_PropertyWorkflowHistory_ActionBy] FOREIGN KEY ([ActionById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- GIS SYNC LOG
-- =============================================
CREATE TABLE [dbo].[GisSyncLogs] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [PropertyId]            INT             NOT NULL,
    [Direction]             TINYINT         NOT NULL DEFAULT 0,    -- 0=ToGis, 1=FromGis
    [Status]                TINYINT         NOT NULL DEFAULT 0,    -- enum GisSyncStatus
    [RequestPayload]        NVARCHAR(MAX)   NULL,
    [ResponsePayload]       NVARCHAR(MAX)   NULL,
    [ErrorMessage]          NVARCHAR(MAX)   NULL,
    [AttemptedAt]           DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [CompletedAt]           DATETIME2       NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    CONSTRAINT [PK_GisSyncLogs] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_GisSyncLogs_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [dbo].[Properties]([Id])
);
GO

-- =============================================
-- INSPECTION MISSIONS
-- =============================================
CREATE TABLE [dbo].[InspectionMissions] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [MissionNumber]         NVARCHAR(50)    NOT NULL,
    [Title]                 NVARCHAR(300)   COLLATE Arabic_CI_AS NOT NULL,
    [Description]           NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [MissionType]           TINYINT         NOT NULL DEFAULT 0,    -- enum MissionType
    [GovernorateId]         INT             NULL,
    [DistrictId]            INT             NULL,
    [AssignedTeamId]        INT             NULL,
    [AssignedToId]          INT             NULL,
    [SupervisorId]          INT             NULL,
    [ScheduledDate]         DATE            NOT NULL,
    [StartedAt]             DATETIME2       NULL,
    [CompletedAt]           DATETIME2       NULL,
    [Status]                TINYINT         NOT NULL DEFAULT 0,    -- enum MissionStatus
    [TargetPropertyCount]   INT             NULL,
    [CompletedPropertyCount] INT            NULL DEFAULT 0,
    [CheckInLatitude]       DECIMAL(10,7)   NULL,
    [CheckInLongitude]      DECIMAL(10,7)   NULL,
    [CheckInAt]             DATETIME2       NULL,
    [Notes]                 NVARCHAR(MAX)   COLLATE Arabic_CI_AS NULL,
    [ProgressPercent]       DECIMAL(5,2)    NOT NULL DEFAULT 0,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    [CreatedById]           INT             NOT NULL,
    CONSTRAINT [PK_InspectionMissions] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_InspectionMissions_Number] UNIQUE ([MissionNumber]),
    CONSTRAINT [FK_InspectionMissions_Governorate] FOREIGN KEY ([GovernorateId]) REFERENCES [dbo].[Governorates]([Id]),
    CONSTRAINT [FK_InspectionMissions_District] FOREIGN KEY ([DistrictId]) REFERENCES [dbo].[Districts]([Id]),
    CONSTRAINT [FK_InspectionMissions_AssignedTo] FOREIGN KEY ([AssignedToId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_InspectionMissions_Supervisor] FOREIGN KEY ([SupervisorId]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_InspectionMissions_CreatedBy] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- AUDIT LOGS
-- =============================================
CREATE TABLE [dbo].[AuditLogs] (
    [Id]                    BIGINT          IDENTITY(1,1) NOT NULL,
    [TableName]             NVARCHAR(100)   NOT NULL,
    [RecordId]              INT             NOT NULL,
    [Action]                NVARCHAR(20)    NOT NULL,              -- INSERT/UPDATE/DELETE
    [OldValues]             NVARCHAR(MAX)   NULL,                  -- JSON
    [NewValues]             NVARCHAR(MAX)   NULL,                  -- JSON
    [ChangedColumns]        NVARCHAR(MAX)   NULL,                  -- JSON array
    [UserId]                INT             NULL,
    [IpAddress]             NVARCHAR(50)    NULL,
    [UserAgent]             NVARCHAR(500)   NULL,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_AuditLogs_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- NOTIFICATIONS
-- =============================================
CREATE TABLE [dbo].[Notifications] (
    [Id]                    INT             IDENTITY(1,1) NOT NULL,
    [UserId]                INT             NOT NULL,
    [Title]                 NVARCHAR(200)   COLLATE Arabic_CI_AS NOT NULL,
    [Message]               NVARCHAR(MAX)   COLLATE Arabic_CI_AS NOT NULL,
    [NotificationType]      TINYINT         NOT NULL DEFAULT 0,    -- enum NotificationType
    [ReferenceTable]        NVARCHAR(100)   NULL,
    [ReferenceId]           INT             NULL,
    [IsRead]                BIT             NOT NULL DEFAULT 0,
    [ReadAt]                DATETIME2       NULL,
    [IsDeleted]             BIT             NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2       NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
);
GO

-- =============================================
-- PERFORMANCE INDEXES
-- =============================================

-- Properties indexes
CREATE NONCLUSTERED INDEX [IX_Properties_GovernorateId] ON [dbo].[Properties] ([GovernorateId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_ApprovalStage] ON [dbo].[Properties] ([ApprovalStage]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_PropertyStatus] ON [dbo].[Properties] ([PropertyStatus]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_PropertyType] ON [dbo].[Properties] ([PropertyType]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_OwnershipType] ON [dbo].[Properties] ([OwnershipType]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_GisFeatureId] ON [dbo].[Properties] ([GisFeatureId]) WHERE [GisFeatureId] IS NOT NULL;
CREATE NONCLUSTERED INDEX [IX_Properties_GisSyncStatus] ON [dbo].[Properties] ([GisSyncStatus]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_DqsScore] ON [dbo].[Properties] ([DqsScore]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_CreatedById] ON [dbo].[Properties] ([CreatedById]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Properties_LocalId] ON [dbo].[Properties] ([LocalId]) WHERE [LocalId] IS NOT NULL;
CREATE NONCLUSTERED INDEX [IX_Properties_LatLong] ON [dbo].[Properties] ([Latitude], [Longitude]) WHERE [Latitude] IS NOT NULL AND [Longitude] IS NOT NULL;
GO

-- Geographic indexes
CREATE NONCLUSTERED INDEX [IX_Governorates_CountryId] ON [dbo].[Governorates] ([CountryId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Districts_GovernorateId] ON [dbo].[Districts] ([GovernorateId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_SubDistricts_DistrictId] ON [dbo].[SubDistricts] ([DistrictId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Neighborhoods_SubDistrictId] ON [dbo].[Neighborhoods] ([SubDistrictId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Streets_NeighborhoodId] ON [dbo].[Streets] ([NeighborhoodId]) WHERE [IsDeleted] = 0;
GO

-- Building component indexes
CREATE NONCLUSTERED INDEX [IX_PropertyFloors_PropertyId] ON [dbo].[PropertyFloors] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyUnits_FloorId] ON [dbo].[PropertyUnits] ([FloorId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyUnits_PropertyId] ON [dbo].[PropertyUnits] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyUnits_OccupancyStatus] ON [dbo].[PropertyUnits] ([OccupancyStatus]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyRooms_UnitId] ON [dbo].[PropertyRooms] ([UnitId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyFacilities_PropertyId] ON [dbo].[PropertyFacilities] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyMeters_UnitId] ON [dbo].[PropertyMeters] ([UnitId]) WHERE [IsDeleted] = 0;
GO

-- Partnership & Agricultural indexes
CREATE NONCLUSTERED INDEX [IX_PropertyPartnerships_PropertyId] ON [dbo].[PropertyPartnerships] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_AgriculturalDetails_PropertyId] ON [dbo].[AgriculturalDetails] ([PropertyId]) WHERE [IsDeleted] = 0;
GO

-- Documents & Photos indexes
CREATE NONCLUSTERED INDEX [IX_PropertyDocuments_PropertyId] ON [dbo].[PropertyDocuments] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyDocuments_DocumentCategory] ON [dbo].[PropertyDocuments] ([DocumentCategory]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyPhotos_PropertyId] ON [dbo].[PropertyPhotos] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyPhotos_UnitId] ON [dbo].[PropertyPhotos] ([UnitId]) WHERE [UnitId] IS NOT NULL AND [IsDeleted] = 0;
GO

-- Workflow & GIS indexes
CREATE NONCLUSTERED INDEX [IX_PropertyWorkflowHistory_PropertyId] ON [dbo].[PropertyWorkflowHistory] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_GisSyncLogs_PropertyId] ON [dbo].[GisSyncLogs] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_GisSyncLogs_Status] ON [dbo].[GisSyncLogs] ([Status]);
GO

-- Mission indexes
CREATE NONCLUSTERED INDEX [IX_InspectionMissions_GovernorateId] ON [dbo].[InspectionMissions] ([GovernorateId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_InspectionMissions_AssignedToId] ON [dbo].[InspectionMissions] ([AssignedToId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_InspectionMissions_ScheduledDate] ON [dbo].[InspectionMissions] ([ScheduledDate]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_InspectionMissions_Status] ON [dbo].[InspectionMissions] ([Status]) WHERE [IsDeleted] = 0;
GO

-- Audit & Notification indexes
CREATE NONCLUSTERED INDEX [IX_AuditLogs_TableName_RecordId] ON [dbo].[AuditLogs] ([TableName], [RecordId]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs] ([CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_IsRead] ON [dbo].[Notifications] ([UserId], [IsRead]) WHERE [IsDeleted] = 0;
GO

-- Users indexes
CREATE NONCLUSTERED INDEX [IX_Users_RoleId] ON [dbo].[Users] ([RoleId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_Users_GovernorateId] ON [dbo].[Users] ([GovernorateId]) WHERE [GovernorateId] IS NOT NULL AND [IsDeleted] = 0;
GO

-- PropertyAddresses indexes
CREATE NONCLUSTERED INDEX [IX_PropertyAddresses_PropertyId] ON [dbo].[PropertyAddresses] ([PropertyId]) WHERE [IsDeleted] = 0;
CREATE NONCLUSTERED INDEX [IX_PropertyAddresses_StreetId] ON [dbo].[PropertyAddresses] ([StreetId]) WHERE [StreetId] IS NOT NULL AND [IsDeleted] = 0;
GO

-- =============================================
-- SEED DATA
-- =============================================

-- Seed Country: Iraq
SET IDENTITY_INSERT [dbo].[Countries] ON;
INSERT INTO [dbo].[Countries] ([Id], [NameAr], [NameEn], [Code]) VALUES (1, N'العراق', N'Iraq', N'IQ');
SET IDENTITY_INSERT [dbo].[Countries] OFF;
GO

-- Seed 18 Iraqi Governorates
SET IDENTITY_INSERT [dbo].[Governorates] ON;
INSERT INTO [dbo].[Governorates] ([Id], [CountryId], [NameAr], [NameEn], [Code]) VALUES
(1,  1, N'بغداد',          N'Baghdad',         N'BGW'),
(2,  1, N'البصرة',         N'Basra',            N'BSA'),
(3,  1, N'نينوى',          N'Nineveh',          N'NIN'),
(4,  1, N'أربيل',          N'Erbil',            N'EBL'),
(5,  1, N'السليمانية',     N'Sulaymaniyah',     N'SLM'),
(6,  1, N'دهوك',           N'Duhok',            N'DHK'),
(7,  1, N'كركوك',          N'Kirkuk',           N'KRK'),
(8,  1, N'ديالى',          N'Diyala',           N'DIY'),
(9,  1, N'الأنبار',        N'Anbar',            N'ANB'),
(10, 1, N'بابل',           N'Babylon',          N'BAB'),
(11, 1, N'كربلاء',         N'Karbala',          N'KAR'),
(12, 1, N'النجف',          N'Najaf',            N'NAJ'),
(13, 1, N'واسط',           N'Wasit',            N'WAS'),
(14, 1, N'صلاح الدين',     N'Salahuddin',       N'SLD'),
(15, 1, N'ذي قار',         N'Dhi Qar',          N'DHQ'),
(16, 1, N'ميسان',          N'Maysan',           N'MYS'),
(17, 1, N'المثنى',         N'Muthanna',         N'MTN'),
(18, 1, N'القادسية',       N'Qadisiyah',        N'QAD');
SET IDENTITY_INSERT [dbo].[Governorates] OFF;
GO

-- Seed Roles
SET IDENTITY_INSERT [dbo].[Roles] ON;
INSERT INTO [dbo].[Roles] ([Id], [NameAr], [NameEn], [Code]) VALUES
(1,  N'مدير النظام',            N'System Administrator',     N'SYS_ADMIN'),
(2,  N'مدير الهيئة',            N'Authority Director',       N'AUTH_DIRECTOR'),
(3,  N'مدير إقليمي',            N'Regional Manager',         N'REGIONAL_MGR'),
(4,  N'مراجع قانوني',           N'Legal Reviewer',           N'LEGAL_REVIEWER'),
(5,  N'مهندس',                  N'Engineer',                 N'ENGINEER'),
(6,  N'مشرف ميداني',            N'Field Supervisor',         N'FIELD_SUPERVISOR'),
(7,  N'باحث ميداني',            N'Field Inspector',          N'FIELD_INSPECTOR'),
(8,  N'جابي',                   N'Collector',                N'COLLECTOR'),
(9,  N'مدير العقود',            N'Contracts Manager',        N'CONTRACTS_MGR'),
(10, N'محلل',                   N'Analyst',                  N'ANALYST');
SET IDENTITY_INSERT [dbo].[Roles] OFF;
GO

-- Seed default system admin user (password: Admin@123)
SET IDENTITY_INSERT [dbo].[Users] ON;
INSERT INTO [dbo].[Users] ([Id], [FullNameAr], [FullNameEn], [Email], [PasswordHash], [RoleId], [IsActive])
VALUES (1, N'مدير النظام', N'System Admin', N'admin@waqf.gov.iq',
        N'AQAAAAIAAYagAAAAEGz0sMwqOENPjVNPkGwV3Q==', 1, 1);
SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

PRINT N'=== Schema creation completed successfully ===';
GO
