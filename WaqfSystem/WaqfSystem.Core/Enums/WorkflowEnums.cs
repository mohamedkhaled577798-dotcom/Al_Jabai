namespace WaqfSystem.Core.Enums
{
    /// <summary>مراحل الموافقة</summary>
    public enum ApprovalStage : byte
    {
        Draft = 0,                   // مسودة
        FieldSupervisorReview = 1,   // مراجعة المشرف الميداني
        LegalReview = 2,             // المراجعة القانونية
        EngineeringReview = 3,       // المراجعة الهندسية
        RegionalApproval = 4,        // الموافقة الإقليمية
        Approved = 5,                // موافق عليه
        SentForCorrection = 6        // أعيد للتصحيح
    }

    /// <summary>حالة المهمة (متوافق مع الواجهات القديمة)</summary>
    public enum MissionStatus : byte
    {
        Planned = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3,
        Postponed = 4
    }

    /// <summary>نوع الإشعار</summary>
    public enum NotificationType : byte
    {
        WorkflowAction = 0,         // إجراء سير العمل
        MissionAssignment = 1,      // تعيين مهمة
        DocumentExpiry = 2,         // انتهاء صلاحية مستند
        GisSyncResult = 3,          // نتيجة مزامنة GIS
        SystemAlert = 4,            // تنبيه نظام
        ApprovalRequired = 5        // موافقة مطلوبة
    }
}
