namespace WaqfSystem.Core.Enums
{
    /// <summary>مستوى النطاق الجغرافي للدور/المستخدم</summary>
    public enum GeographicScopeLevel : byte
    {
        None = 0,
        Governorate = 1,
        District = 2,
        SubDistrict = 3
    }

    /// <summary>أدوار المستخدمين</summary>
    public enum UserRole : byte
    {
        SysAdmin = 0,               // مدير النظام
        AuthDirector = 1,           // مدير الهيئة
        RegionalMgr = 2,            // مدير إقليمي
        LegalReviewer = 3,          // مراجع قانوني
        Engineer = 4,               // مهندس
        FieldSupervisor = 5,        // مشرف ميداني
        FieldInspector = 6,         // باحث ميداني
        Collector = 7,              // جابي
        ContractsMgr = 8,           // مدير العقود
        Analyst = 9                 // محلل
    }
}
