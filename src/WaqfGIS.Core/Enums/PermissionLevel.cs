namespace WaqfGIS.Core.Enums;

/// <summary>
/// أنواع الصلاحيات في النظام
/// </summary>
public enum PermissionLevel
{
    /// <summary>
    /// صلاحية كاملة على جميع البيانات
    /// </summary>
    SuperAdmin = 0,

    /// <summary>
    /// صلاحية إدارية على جميع البيانات
    /// </summary>
    Admin = 1,

    /// <summary>
    /// صلاحية على مستوى المحافظة فقط
    /// </summary>
    ProvinceLevel = 2,

    /// <summary>
    /// صلاحية على مستوى الدائرة الوقفية فقط
    /// </summary>
    OfficeLevel = 3,

    /// <summary>
    /// صلاحية عرض فقط
    /// </summary>
    ViewOnly = 4
}

/// <summary>
/// أنواع العمليات
/// </summary>
public enum OperationType
{
    View = 0,
    Create = 1,
    Edit = 2,
    Delete = 3,
    Export = 4
}
