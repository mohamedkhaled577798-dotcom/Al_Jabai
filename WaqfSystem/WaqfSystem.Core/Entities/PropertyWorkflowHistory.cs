using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// سجل سير العمل — Property workflow transition history.
    /// </summary>
    public class PropertyWorkflowHistory : BaseEntity
    {
        public int PropertyId { get; set; }
        public ApprovalStage FromStage { get; set; }
        public ApprovalStage ToStage { get; set; }
        public int ActionById { get; set; }
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public decimal? DqsAtAction { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public virtual User ActionBy { get; set; } = null!;
    }
}
