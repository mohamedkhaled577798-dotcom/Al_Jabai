namespace WaqfGIS.Web.Models;

/// <summary>
/// Model للـ Partial View الخاص باختيار الكيان الوقفي
/// </summary>
public class WaqfEntitySelectorModel
{
    /// <summary>معرّف HTML فريد لتجنب التكرار في نفس الصفحة</summary>
    public string? UniqueId { get; set; }

    /// <summary>اسم الحقل المخفي لنوع الكيان (EntityType)</summary>
    public string EntityTypeFieldId { get; set; } = "EntityType";

    /// <summary>اسم الحقل المخفي لمعرّف الكيان (EntityId)</summary>
    public string EntityIdFieldId { get; set; } = "EntityId";

    /// <summary>اسم الحقل المخفي لاسم الكيان (EntityName)</summary>
    public string EntityNameFieldId { get; set; } = "EntityName";

    /// <summary>القيمة الحالية لنوع الكيان (للتعديل)</summary>
    public string? CurrentEntityType { get; set; }

    /// <summary>القيمة الحالية لمعرّف الكيان (للتعديل)</summary>
    public int CurrentEntityId { get; set; }

    /// <summary>القيمة الحالية لاسم الكيان (للتعديل)</summary>
    public string? CurrentEntityName { get; set; }
}
