/* Partnership Full Upgrade */

/* Expand enum checks */
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PartnershipType')
BEGIN
    ALTER TABLE PropertyPartnerships DROP CONSTRAINT CK_PartnershipType;
END;
GO

ALTER TABLE PropertyPartnerships
ADD CONSTRAINT CK_PartnershipType CHECK (
    PartnershipType IN (
        'RevenuePercent','FloorOwnership','UnitOwnership','UsufructRight',
        'LandPercent','TimedPartnership','HarvestShare','Custom'
    )
);
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ExpenseBearingMethod')
BEGIN
    ALTER TABLE PropertyPartnerships DROP CONSTRAINT CK_ExpenseBearingMethod;
END;
GO

IF COL_LENGTH('PropertyPartnerships', 'ExpenseBearingMethod') IS NULL
BEGIN
    ALTER TABLE PropertyPartnerships
    ADD ExpenseBearingMethod NVARCHAR(30) COLLATE Arabic_CI_AS NOT NULL
        CONSTRAINT DF_PropertyPartnerships_ExpenseBearingMethod DEFAULT 'BeforeDistribution';
END;
GO

ALTER TABLE PropertyPartnerships
ADD CONSTRAINT CK_ExpenseBearingMethod CHECK (
    ExpenseBearingMethod IN ('BeforeDistribution','SharedByPercent','WaqfOnly','PartnerOnly')
);
GO

IF COL_LENGTH('PropertyPartnerships', 'CustomPartnershipName') IS NULL
BEGIN
    ALTER TABLE PropertyPartnerships ADD CustomPartnershipName NVARCHAR(120) COLLATE Arabic_CI_AS NULL;
END;
GO

IF COL_LENGTH('PropertyPartnerships', 'CustomCalculationFormula') IS NULL
BEGIN
    ALTER TABLE PropertyPartnerships ADD CustomCalculationFormula NVARCHAR(1000) COLLATE Arabic_CI_AS NULL;
END;
GO

IF COL_LENGTH('PartnerRevenueDistributions', 'TotalExpenses') IS NULL
BEGIN
    ALTER TABLE PartnerRevenueDistributions ADD TotalExpenses DECIMAL(15,2) NOT NULL CONSTRAINT DF_PartnerRevenueDistributions_TotalExpenses DEFAULT 0;
END;
GO

IF COL_LENGTH('PartnerRevenueDistributions', 'NetRevenue') IS NULL
BEGIN
    ALTER TABLE PartnerRevenueDistributions ADD NetRevenue DECIMAL(15,2) NOT NULL CONSTRAINT DF_PartnerRevenueDistributions_NetRevenue DEFAULT 0;
END;
GO

IF OBJECT_ID('PartnershipConditionRules', 'U') IS NULL
BEGIN
    CREATE TABLE PartnershipConditionRules (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PartnershipId INT NOT NULL REFERENCES PropertyPartnerships(Id) ON DELETE CASCADE,
        RuleType NVARCHAR(40) COLLATE Arabic_CI_AS NOT NULL,
        Scope NVARCHAR(40) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'Always',
        RuleName NVARCHAR(200) COLLATE Arabic_CI_AS NOT NULL,
        FixedAmount DECIMAL(15,2) NULL,
        PercentValue DECIMAL(5,2) NULL,
        MinRevenueThreshold DECIMAL(15,2) NULL,
        MaxRevenueThreshold DECIMAL(15,2) NULL,
        StartDate DATE NULL,
        EndDate DATE NULL,
        DistributionType NVARCHAR(30) COLLATE Arabic_CI_AS NULL,
        SeasonLabel NVARCHAR(60) COLLATE Arabic_CI_AS NULL,
        PriorityOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedById INT NULL REFERENCES Users(Id)
    );
END;
GO

IF OBJECT_ID('PartnershipExpenseEntries', 'U') IS NULL
BEGIN
    CREATE TABLE PartnershipExpenseEntries (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PartnershipId INT NOT NULL REFERENCES PropertyPartnerships(Id) ON DELETE CASCADE,
        PropertyId INT NOT NULL REFERENCES Properties(Id),
        PeriodLabel NVARCHAR(50) COLLATE Arabic_CI_AS NOT NULL,
        PeriodStartDate DATE NOT NULL,
        PeriodEndDate DATE NOT NULL,
        ExpenseType NVARCHAR(80) COLLATE Arabic_CI_AS NOT NULL,
        Amount DECIMAL(15,2) NOT NULL,
        ReferenceNo NVARCHAR(100) COLLATE Arabic_CI_AS NULL,
        Notes NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedById INT NULL REFERENCES Users(Id)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ConditionRule_Partnership_Active' AND object_id = OBJECT_ID('PartnershipConditionRules'))
BEGIN
    CREATE INDEX IX_ConditionRule_Partnership_Active ON PartnershipConditionRules(PartnershipId, IsActive, PriorityOrder);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Expense_Partnership_Period' AND object_id = OBJECT_ID('PartnershipExpenseEntries'))
BEGIN
    CREATE INDEX IX_Expense_Partnership_Period ON PartnershipExpenseEntries(PartnershipId, PeriodStartDate, PeriodEndDate);
END;
GO
