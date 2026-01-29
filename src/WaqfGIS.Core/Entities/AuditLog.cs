using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// سجل التدقيق
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// تجاوز Id ليكون long لأن الجدول الأصلي bigint
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new long Id { get; set; }

    public string? UserId { get; set; }
    
    /// <summary>
    /// اسم المستخدم - يتم حفظه مباشرة للتسهيل
    /// </summary>
    public string? UserName { get; set; }
    
    public string Action { get; set; } = string.Empty; // إنشاء, تعديل, حذف
    
    /// <summary>
    /// نوع الكيان (للتوافق مع الحقل القديم TableName)
    /// </summary>
    [Column("TableName")]
    public string? EntityType { get; set; } // Mosque, Property, Office
    
    /// <summary>
    /// معرف الكيان (للتوافق مع الحقل القديم RecordId)
    /// </summary>
    [Column("RecordId")]
    public int? EntityId { get; set; }
    
    /// <summary>
    /// اسم الكيان للعرض
    /// </summary>
    public string? EntityName { get; set; }
    
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation Properties
    public virtual ApplicationUser? User { get; set; }
}
