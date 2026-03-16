-- 007_GeographicRoleScoping.sql
-- Adds role geographic scope and user geographic scope assignments.

IF COL_LENGTH('Roles', 'GeographicScopeLevel') IS NULL
BEGIN
    ALTER TABLE Roles ADD GeographicScopeLevel TINYINT NOT NULL CONSTRAINT DF_Roles_GeographicScopeLevel DEFAULT(0);
END
GO

IF COL_LENGTH('Roles', 'HasGlobalScope') IS NULL
BEGIN
    ALTER TABLE Roles ADD HasGlobalScope BIT NOT NULL CONSTRAINT DF_Roles_HasGlobalScope DEFAULT(0);
END
GO

IF COL_LENGTH('Users', 'DistrictId') IS NULL
BEGIN
    ALTER TABLE Users ADD DistrictId INT NULL;
END
GO

IF COL_LENGTH('Users', 'SubDistrictId') IS NULL
BEGIN
    ALTER TABLE Users ADD SubDistrictId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_District' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_District ON Users(DistrictId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_SubDistrict' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_SubDistrict ON Users(SubDistrictId);
END
GO

IF OBJECT_ID('UserGeographicScopes', 'U') IS NULL
BEGIN
    CREATE TABLE UserGeographicScopes
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserGeographicScopes PRIMARY KEY,
        UserId INT NOT NULL,
        ScopeLevel TINYINT NOT NULL,
        GovernorateId INT NULL,
        DistrictId INT NULL,
        SubDistrictId INT NULL,
        IsPrimary BIT NOT NULL CONSTRAINT DF_UserGeographicScopes_IsPrimary DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_UserGeographicScopes_IsActive DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_UserGeographicScopes_CreatedAt DEFAULT(SYSUTCDATETIME()),
        CreatedById INT NULL,
        CONSTRAINT FK_UserGeographicScopes_User FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserGeographicScopes_Gov FOREIGN KEY(GovernorateId) REFERENCES Governorates(Id),
        CONSTRAINT FK_UserGeographicScopes_District FOREIGN KEY(DistrictId) REFERENCES Districts(Id),
        CONSTRAINT FK_UserGeographicScopes_SubDistrict FOREIGN KEY(SubDistrictId) REFERENCES SubDistricts(Id),
        CONSTRAINT FK_UserGeographicScopes_CreatedBy FOREIGN KEY(CreatedById) REFERENCES Users(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserGeoScope_UserScope' AND object_id = OBJECT_ID('UserGeographicScopes'))
BEGIN
    CREATE INDEX IX_UserGeoScope_UserScope
        ON UserGeographicScopes(UserId, ScopeLevel, GovernorateId, DistrictId, SubDistrictId);
END
GO
