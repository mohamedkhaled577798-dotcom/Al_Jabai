ALTER TABLE PropertyRevenues ADD
    BatchId NVARCHAR(30) NULL,
    SuggestedBySystem BIT NOT NULL CONSTRAINT DF_PropertyRevenues_SuggestedBySystem DEFAULT(0),
    ConfirmedAt DATETIME2 NULL,
    VarianceApprovedBy INT NULL,
    VarianceApprovalNote NVARCHAR(300) COLLATE Arabic_CI_AS NULL;

CREATE TABLE CollectionBatches
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    BatchCode NVARCHAR(30) NOT NULL,
    PeriodLabel NVARCHAR(50) COLLATE Arabic_CI_AS NOT NULL,
    CollectedById INT NOT NULL,
    TotalAmount DECIMAL(15,2) NOT NULL CONSTRAINT DF_CollectionBatches_TotalAmount DEFAULT(0),
    ItemCount INT NOT NULL CONSTRAINT DF_CollectionBatches_ItemCount DEFAULT(0),
    CollectionDate DATE NOT NULL,
    PaymentMethod NVARCHAR(30) NULL,
    Notes NVARCHAR(300) COLLATE Arabic_CI_AS NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_CollectionBatches_CreatedAt DEFAULT(GETUTCDATE()),
    CONSTRAINT FK_CollectionBatches_Users_CollectedById FOREIGN KEY (CollectedById) REFERENCES Users(Id)
);

CREATE TABLE CollectionSmartLog
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    SuggestionType NVARCHAR(30) NOT NULL,
    UnitId BIGINT NULL,
    FloorId BIGINT NULL,
    PropertyId BIGINT NOT NULL,
    WasActedOn BIT NOT NULL CONSTRAINT DF_CollectionSmartLog_WasActedOn DEFAULT(0),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_CollectionSmartLog_CreatedAt DEFAULT(GETUTCDATE()),
    CONSTRAINT FK_CollectionSmartLog_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_CollectionSmartLog_PropertyUnits_UnitId FOREIGN KEY (UnitId) REFERENCES PropertyUnits(Id),
    CONSTRAINT FK_CollectionSmartLog_PropertyFloors_FloorId FOREIGN KEY (FloorId) REFERENCES PropertyFloors(Id),
    CONSTRAINT FK_CollectionSmartLog_Properties_PropertyId FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
);

CREATE INDEX IX_Batch_Period ON CollectionBatches(PeriodLabel);
CREATE INDEX IX_Revenue_Batch ON PropertyRevenues(BatchId);
CREATE INDEX IX_SmartLog_User ON CollectionSmartLog(UserId, CreatedAt);
