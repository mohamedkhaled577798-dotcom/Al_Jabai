ALTER TABLE PropertyPartnerships ADD
  PartnershipType NVARCHAR(30) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'RevenuePercent'
    CONSTRAINT CK_PartnershipType CHECK (PartnershipType IN
      ('RevenuePercent','FloorOwnership','UnitOwnership','UsufructRight',
       'LandPercent','TimedPartnership','HarvestShare')),
  OwnedFloorNumbers NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  OwnedUnitIds NVARCHAR(MAX) COLLATE Arabic_CI_AS NULL,
  UsufructStartDate DATE NULL,
  UsufructEndDate DATE NULL,
  UsufructTermYears INT NULL,
  UsufructAnnualFeePerYear DECIMAL(15,2) NULL,
  PartnershipStartDate DATE NULL,
  PartnershipEndDate DATE NULL,
  LandSharePercentWaqf DECIMAL(5,2) NULL,
  LandTotalDunum DECIMAL(10,2) NULL,
  WaqfLandDunum DECIMAL(10,2) NULL,
  WaqfHarvestPercent DECIMAL(5,2) NULL,
  FarmerName NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  FarmerNationalId NVARCHAR(20) COLLATE Arabic_CI_AS NULL,
  HarvestContractType NVARCHAR(20) COLLATE Arabic_CI_AS NULL,
  PartnerNameEn NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  PartnerType NVARCHAR(30) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'Individual'
    CONSTRAINT CK_PartnerType CHECK (PartnerType IN
      ('Individual','Company','Heirs','Government','Foundation','Other')),
  PartnerRegistrationNo NVARCHAR(50) COLLATE Arabic_CI_AS NULL,
  PartnerPhone2 NVARCHAR(20) COLLATE Arabic_CI_AS NULL,
  PartnerEmail NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  PartnerWhatsApp NVARCHAR(20) COLLATE Arabic_CI_AS NULL,
  PartnerAddress NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  PartnerBankName NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  PartnerBankIBAN NVARCHAR(50) COLLATE Arabic_CI_AS NULL,
  PartnerBankAccountNo NVARCHAR(50) COLLATE Arabic_CI_AS NULL,
  PartnerBankBranch NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  AgreementDate DATE NULL,
  AgreementNotaryName NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  AgreementCourt NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  AgreementReferenceNo NVARCHAR(100) COLLATE Arabic_CI_AS NULL,
  AgreementDocUrl NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  RevenueDistribMethod NVARCHAR(20) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'Monthly'
    CONSTRAINT CK_DistribMethod CHECK (RevenueDistribMethod IN
      ('Monthly','Quarterly','Annual','PerCollection')),
  RevenueDistribDay INT NULL,
  LastDistribDate DATE NULL,
  NextDistribDate DATE NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  DeactivationReason NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  DeactivatedAt DATETIME2 NULL,
  UpdatedAt DATETIME2 NULL,
  CreatedById INT NULL;

CREATE TABLE PartnerRevenueDistributions (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  PartnershipId INT NOT NULL REFERENCES PropertyPartnerships(Id),
  PropertyId INT NOT NULL REFERENCES Properties(Id),
  PeriodLabel NVARCHAR(50) COLLATE Arabic_CI_AS NOT NULL,
  PeriodStartDate DATE NOT NULL,
  PeriodEndDate DATE NOT NULL,
  DistributionType NVARCHAR(20) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'Revenue',
  TotalRevenue DECIMAL(15,2) NOT NULL,
  WaqfAmount DECIMAL(15,2) NOT NULL,
  PartnerAmount DECIMAL(15,2) NOT NULL,
  WaqfPercentSnapshot DECIMAL(5,2) NOT NULL,
  TransferStatus NVARCHAR(20) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'Pending'
    CONSTRAINT CK_TransferStatus CHECK (TransferStatus IN ('Pending','Transferred','Cancelled')),
  TransferDate DATE NULL,
  TransferMethod NVARCHAR(100) COLLATE Arabic_CI_AS NULL,
  TransferReference NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  TransferBankName NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  Notes NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  CreatedById INT NULL
);

CREATE TABLE PartnerContactLogs (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  PartnershipId INT NOT NULL REFERENCES PropertyPartnerships(Id),
  ContactType NVARCHAR(20) COLLATE Arabic_CI_AS NOT NULL
    CONSTRAINT CK_ContactType CHECK (ContactType IN
      ('SMS','WhatsApp','Email','Phone','Meeting','Letter','PDF')),
  ContactDirection NVARCHAR(20) COLLATE Arabic_CI_AS NOT NULL DEFAULT 'Outgoing'
    CONSTRAINT CK_Direction CHECK (ContactDirection IN ('Outgoing','Incoming')),
  Subject NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  MessageBody NVARCHAR(2000) COLLATE Arabic_CI_AS NULL,
  RecipientAddress NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  SentById INT NOT NULL,
  IsAutomatic BIT NOT NULL DEFAULT 0,
  DeliveryStatus NVARCHAR(50) COLLATE Arabic_CI_AS NULL,
  ExternalMessageId NVARCHAR(200) COLLATE Arabic_CI_AS NULL,
  LinkedDistributionId BIGINT NULL REFERENCES PartnerRevenueDistributions(Id),
  Notes NVARCHAR(500) COLLATE Arabic_CI_AS NULL
);

CREATE TABLE PartnerNotificationSchedules (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  PartnershipId INT NOT NULL REFERENCES PropertyPartnerships(Id),
  TriggerType NVARCHAR(50) COLLATE Arabic_CI_AS NOT NULL,
  TriggerDate DATE NOT NULL,
  Channels NVARCHAR(200) COLLATE Arabic_CI_AS NOT NULL,
  IsSent BIT NOT NULL DEFAULT 0,
  SentAt DATETIME2 NULL,
  TemplateKey NVARCHAR(100) COLLATE Arabic_CI_AS NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Partnership_Property ON PropertyPartnerships(PropertyId, IsActive, IsDeleted);
CREATE INDEX IX_Partnership_Expiry ON PropertyPartnerships(PartnershipEndDate, IsActive)
  WHERE PartnershipEndDate IS NOT NULL;
CREATE INDEX IX_Partnership_Usufruct ON PropertyPartnerships(UsufructEndDate, IsActive)
  WHERE UsufructEndDate IS NOT NULL;
CREATE INDEX IX_RevenueDistrib_Partnership ON PartnerRevenueDistributions(PartnershipId, PeriodStartDate);
CREATE INDEX IX_RevenueDistrib_Status ON PartnerRevenueDistributions(TransferStatus, CreatedAt);
CREATE INDEX IX_ContactLog_Partnership ON PartnerContactLogs(PartnershipId, SentAt DESC);
CREATE INDEX IX_NotifSchedule_Pending ON PartnerNotificationSchedules(TriggerDate, IsSent)
  WHERE IsSent = 0;
