namespace WaqfSystem.Core.Enums
{
    public enum MissionStage
    {
        Created,
        Assigned,
        Accepted,
        InProgress,
        DataEntry,
        SubmittedForReview,
        UnderReview,
        Completed,
        SentForCorrection,
        Cancelled,
        Rejected
    }

    public enum MissionType
    {
        PropertyCensus,
        PeriodicInspection,
        DocumentVerification,
        EmergencyAssessment,
        FollowUp
    }

    public enum MissionPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public enum EntryStatus
    {
        InProgress,
        Submitted,
        UnderReview,
        Approved,
        Rejected
    }
}
