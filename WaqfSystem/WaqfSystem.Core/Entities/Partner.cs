using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// سجل الشركاء والمستثمرين — Central registry for partners and companies.
    /// </summary>
    public class Partner : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public PartnerType Type { get; set; } = PartnerType.Individual;
        
        public string? NationalId { get; set; }
        public string? RegistrationNo { get; set; }
        
        public string Phone { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? WhatsApp { get; set; }
        public string? Address { get; set; }
        
        // Bank Information
        public string? BankName { get; set; }
        public string? BankIBAN { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankBranch { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        public override User? CreatedBy { get; set; }
    }
}
