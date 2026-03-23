namespace WaqfSystem.Core.Enums
{
    /// <summary>
    /// مستوى التحصيل — Collection level (Building, Floor, Unit).
    /// </summary>
    public enum CollectionLevel : byte
    {
        Building = 0,
        Floor = 1,
        Unit = 2
    }

    /// <summary>
    /// حالة العقد — Rent contract status.
    /// </summary>
    public enum ContractStatus : byte
    {
        Draft = 0,
        Active = 1,
        Suspended = 2,
        Terminated = 3,
        Expired = 4
    }

    /// <summary>
    /// حالة الدفع — Payment status for schedules.
    /// </summary>
    public enum PaymentStatus : byte
    {
        Unpaid = 0,
        Partial = 1,
        Paid = 2,
        Overdue = 3,
        Cancelled = 4
    }

    /// <summary>
    /// نوع المقترح الذكي — Smart suggestion type.
    /// </summary>
    public enum SuggestionType : byte
    {
        Overdue = 0,        // متأخرات
        DueToday = 1,       // يستحق اليوم
        DueSoon = 2,        // يستحق قريباً
        UnpaidLastMonth = 3 // لم يدفع الشهر الماضي
    }

    /// <summary>
    /// مستوى التنبيه — Variance alert level.
    /// </summary>
    public enum AlertLevel : byte
    {
        None = 0,
        Warning = 1,
        Critical = 2
    }

    /// <summary>
    /// نوع الزر الذكي للمبلغ — Smart amount chip type.
    /// </summary>
    public enum ChipType : byte
    {
        Full = 0,
        Half = 1,
        WithPenalty = 2,
        Custom = 3
    }

    /// <summary>
    /// فترة التحصيل — Collection period type.
    /// </summary>
    public enum CollectionPeriodType : byte
    {
        Monthly = 0,
        Quarterly = 1,
        Annual = 2
    }
}
