CREATE TABLE InspectionTeams (
  Id INT IDENTITY(1,1) PRIMARY KEY,
  TeamName NVARCHAR(100) COLLATE Arabic_CI_AS NOT NULL,
  TeamCode NVARCHAR(20) NOT NULL,
  GovernorateId INT NOT NULL REFERENCES Governorates(Id),
  LeaderId INT NULL REFERENCES Users(Id),
  Description NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  CreatedById INT NOT NULL,
  IsDeleted BIT NOT NULL DEFAULT 0,
  CONSTRAINT UQ_TeamCode UNIQUE (TeamCode)
);

CREATE TABLE InspectionTeamMembers (
  Id INT IDENTITY(1,1) PRIMARY KEY,
  TeamId INT NOT NULL REFERENCES InspectionTeams(Id),
  UserId INT NOT NULL REFERENCES Users(Id),
  JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  IsActive BIT NOT NULL DEFAULT 1,
  AddedById INT NOT NULL,
  CONSTRAINT UQ_TeamMember UNIQUE (TeamId, UserId)
);

CREATE TABLE InspectionMissions (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  MissionCode NVARCHAR(30) NOT NULL,
  Title NVARCHAR(200) COLLATE Arabic_CI_AS NOT NULL,
  Description NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  MissionType NVARCHAR(30) NOT NULL DEFAULT 'PropertyCensus'
    CONSTRAINT CK_MissionType CHECK (MissionType IN
      ('PropertyCensus','PeriodicInspection','DocumentVerification',
       'EmergencyAssessment','FollowUp')),
  Stage NVARCHAR(30) NOT NULL DEFAULT 'Created'
    CONSTRAINT CK_MissionStage CHECK (Stage IN
      ('Created','Assigned','Accepted','InProgress','DataEntry',
       'SubmittedForReview','UnderReview','Completed',
       'SentForCorrection','Cancelled','Rejected')),
  Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal'
    CONSTRAINT CK_Priority CHECK (Priority IN ('Low','Normal','High','Urgent')),
  GovernorateId INT NOT NULL REFERENCES Governorates(Id),
  DistrictId INT NULL REFERENCES Districts(Id),
  SubDistrictId INT NULL REFERENCES SubDistricts(Id),
  TargetArea NVARCHAR(300) COLLATE Arabic_CI_AS NULL,
  TargetPropertyCount INT NOT NULL DEFAULT 0,
  EnteredPropertyCount INT NOT NULL DEFAULT 0,
  ReviewedPropertyCount INT NOT NULL DEFAULT 0,
  ApprovedPropertyCount INT NOT NULL DEFAULT 0,
  AverageDqsScore DECIMAL(5,2) NULL,
  ProgressPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
  AssignedToUserId INT NULL REFERENCES Users(Id),
  AssignedToTeamId INT NULL REFERENCES InspectionTeams(Id),
  AssignedByUserId INT NULL REFERENCES Users(Id),
  AssignedAt DATETIME2 NULL,
  ReviewerUserId INT NULL REFERENCES Users(Id),
  MissionDate DATE NOT NULL,
  ExpectedCompletionDate DATE NULL,
  ActualCompletionDate DATE NULL,
  CheckinLat DECIMAL(10,7) NULL,
  CheckinLng DECIMAL(10,7) NULL,
  CheckinAt DATETIME2 NULL,
  AcceptedAt DATETIME2 NULL,
  SubmittedAt DATETIME2 NULL,
  CompletedAt DATETIME2 NULL,
  IsUrgent BIT NOT NULL DEFAULT 0,
  AssignmentNotes NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  ReviewNotes NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  CancellationReason NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  RejectionReason NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  CorrectionNotes NVARCHAR(1000) COLLATE Arabic_CI_AS NULL,
  ChecklistTemplateId INT NULL,
  CurrentStageChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  IsDeleted BIT NOT NULL DEFAULT 0,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  CreatedById INT NOT NULL,
  UpdatedAt DATETIME2 NULL,
  CONSTRAINT UQ_MissionCode UNIQUE (MissionCode)
);

CREATE TABLE MissionStageHistory (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  MissionId BIGINT NOT NULL REFERENCES InspectionMissions(Id),
  FromStage NVARCHAR(30) NOT NULL,
  ToStage NVARCHAR(30) NOT NULL,
  ChangedById INT NOT NULL REFERENCES Users(Id),
  ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  Notes NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  TriggerAction NVARCHAR(100) NULL
);

CREATE TABLE MissionPropertyEntries (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  MissionId BIGINT NOT NULL REFERENCES InspectionMissions(Id),
  PropertyId BIGINT NULL REFERENCES Properties(Id),
  LocalId NVARCHAR(100) NULL,
  EnteredByUserId INT NOT NULL REFERENCES Users(Id),
  EntryStartedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  EntryCompletedAt DATETIME2 NULL,
  DqsAtEntry DECIMAL(5,2) NULL,
  EntryStatus NVARCHAR(20) NOT NULL DEFAULT 'InProgress'
    CONSTRAINT CK_EntryStatus CHECK (EntryStatus IN
      ('InProgress','Submitted','UnderReview','Approved','Rejected')),
  ReviewNotes NVARCHAR(500) COLLATE Arabic_CI_AS NULL,
  ReviewedByUserId INT NULL REFERENCES Users(Id),
  ReviewedAt DATETIME2 NULL,
  CONSTRAINT UQ_MissionProperty UNIQUE (MissionId, PropertyId)
);

CREATE TABLE MissionChecklistTemplates (
  Id INT IDENTITY(1,1) PRIMARY KEY,
  TemplateName NVARCHAR(100) COLLATE Arabic_CI_AS NOT NULL,
  MissionType NVARCHAR(30) NULL,
  Items NVARCHAR(MAX) NOT NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  CreatedById INT NOT NULL
);

CREATE TABLE MissionChecklistResults (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  MissionId BIGINT NOT NULL REFERENCES InspectionMissions(Id),
  TemplateId INT NOT NULL REFERENCES MissionChecklistTemplates(Id),
  CompletedByUserId INT NOT NULL REFERENCES Users(Id),
  Results NVARCHAR(MAX) NOT NULL,
  CompletionPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
  SubmittedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Mission_Stage ON InspectionMissions(Stage, MissionDate);
CREATE INDEX IX_Mission_Assigned ON InspectionMissions(AssignedToUserId, Stage);
CREATE INDEX IX_Mission_Gov ON InspectionMissions(GovernorateId, MissionDate);
CREATE INDEX IX_Mission_Date ON InspectionMissions(MissionDate, ExpectedCompletionDate);
CREATE INDEX IX_StageHistory ON MissionStageHistory(MissionId, ChangedAt);
CREATE INDEX IX_Entry_Mission ON MissionPropertyEntries(MissionId, EntryStatus);
CREATE INDEX IX_TeamMember_User ON InspectionTeamMembers(UserId, IsActive);

INSERT INTO MissionChecklistTemplates (TemplateName, MissionType, Items, CreatedById)
VALUES
(N'قائمة تفتيش الأملاك الأساسية', 'PropertyCensus', N'[
  {"id":1,"questionAr":"هل تم تصوير واجهة العقار؟","type":"bool","required":true},
  {"id":2,"questionAr":"هل تم تحديد الإحداثيات بدقة؟","type":"bool","required":true},
  {"id":3,"questionAr":"هل الوثائق القانونية متوفرة؟","type":"bool","required":true},
  {"id":4,"questionAr":"الحالة الإنشائية العامة","type":"rating","required":true,"options":["ممتازة","جيدة","متوسطة","ضعيفة","خطرة"]},
  {"id":5,"questionAr":"ملاحظات إضافية","type":"text","required":false}
]', 1),
(N'قائمة تفتيش دورية', 'PeriodicInspection', N'[
  {"id":1,"questionAr":"هل الوضع الإنشائي تغيّر؟","type":"bool","required":true},
  {"id":2,"questionAr":"هل يوجد تعدٍّ على العقار؟","type":"bool","required":true},
  {"id":3,"questionAr":"هل المستأجر لا يزال في العقار؟","type":"bool","required":false},
  {"id":4,"questionAr":"تقييم الحالة العامة","type":"rating","required":true,"options":["ممتازة","جيدة","متوسطة","ضعيفة","خطرة"]},
  {"id":5,"questionAr":"ملاحظات التفتيش","type":"text","required":true}
]', 1);
