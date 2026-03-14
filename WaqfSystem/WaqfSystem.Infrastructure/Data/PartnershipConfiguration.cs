using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Infrastructure.Data
{
    public class PartnershipConfiguration : IEntityTypeConfiguration<PropertyPartnership>
    {
        public void Configure(EntityTypeBuilder<PropertyPartnership> builder)
        {
            builder.Property(x => x.PartnershipType).HasConversion<string>().HasMaxLength(30).UseCollation("Arabic_CI_AS");
            builder.HasCheckConstraint("CK_PartnershipType", "[PartnershipType] IN ('RevenuePercent','FloorOwnership','UnitOwnership','UsufructRight','LandPercent','TimedPartnership','HarvestShare')");

            builder.Property(x => x.WaqfSharePercent).HasColumnType("decimal(5,2)");
            builder.Property(x => x.PartnerSharePercent).HasColumnType("decimal(5,2)");
            builder.Property(x => x.LandSharePercentWaqf).HasColumnType("decimal(5,2)");
            builder.Property(x => x.LandTotalDunum).HasColumnType("decimal(15,2)");
            builder.Property(x => x.WaqfLandDunum).HasColumnType("decimal(15,2)");
            builder.Property(x => x.WaqfHarvestPercent).HasColumnType("decimal(5,2)");
            builder.Property(x => x.UsufructAnnualFeePerYear).HasColumnType("decimal(15,2)");

            builder.Property(x => x.OwnedFloorNumbers).HasColumnType("nvarchar(500)").UseCollation("Arabic_CI_AS");
            builder.Property(x => x.OwnedUnitIds).HasColumnType("nvarchar(max)").UseCollation("Arabic_CI_AS");

            builder.Property(x => x.PartnerName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerNameEn).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerNationalId).HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerRegistrationNo).HasMaxLength(50).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerPhone).HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerPhone2).HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerEmail).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerWhatsApp).HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerAddress).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerBankName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerBankIBAN).HasMaxLength(50).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerBankAccountNo).HasMaxLength(50).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.PartnerBankBranch).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.AgreementNotaryName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.AgreementCourt).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.AgreementReferenceNo).HasMaxLength(100).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.AgreementDocUrl).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.DeactivationReason).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.FarmerName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.FarmerNationalId).HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.HarvestContractType).HasMaxLength(20).UseCollation("Arabic_CI_AS");

            builder.Property(x => x.RevenueDistribMethod).HasConversion<string>().HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.HasCheckConstraint("CK_DistribMethod", "[RevenueDistribMethod] IN ('Monthly','Quarterly','Annual','PerCollection')");

            builder.HasIndex(x => new { x.PropertyId, x.IsActive }).HasDatabaseName("IX_Partnership_Property_Active");
            builder.HasIndex(x => x.PartnershipEndDate).HasDatabaseName("IX_Partnership_EndDate");
            builder.HasIndex(x => x.UsufructEndDate).HasDatabaseName("IX_Partnership_UsufructEndDate");

            builder.HasMany(x => x.RevenueDistributions)
                .WithOne(x => x.Partnership)
                .HasForeignKey(x => x.PartnershipId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ContactLogs)
                .WithOne(x => x.Partnership)
                .HasForeignKey(x => x.PartnershipId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.NotificationSchedules)
                .WithOne(x => x.Partnership)
                .HasForeignKey(x => x.PartnershipId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PartnerRevenueDistributionConfiguration : IEntityTypeConfiguration<PartnerRevenueDistribution>
    {
        public void Configure(EntityTypeBuilder<PartnerRevenueDistribution> builder)
        {
            builder.ToTable("PartnerRevenueDistributions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PeriodLabel).HasMaxLength(50).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.DistributionType).HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TotalRevenue).HasColumnType("decimal(15,2)");
            builder.Property(x => x.WaqfAmount).HasColumnType("decimal(15,2)");
            builder.Property(x => x.PartnerAmount).HasColumnType("decimal(15,2)");
            builder.Property(x => x.WaqfPercentSnapshot).HasColumnType("decimal(5,2)");
            builder.Property(x => x.TransferStatus).HasConversion<string>().HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TransferMethod).HasMaxLength(100).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TransferReference).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TransferBankName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.Notes).HasMaxLength(500).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => new { x.PartnershipId, x.PeriodStartDate }).HasDatabaseName("IX_RevenueDistrib_Partnership");
            builder.HasIndex(x => x.TransferStatus).HasDatabaseName("IX_RevenueDistrib_Status");

            builder.HasOne(x => x.Property)
                .WithMany()
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PartnerContactLogConfiguration : IEntityTypeConfiguration<PartnerContactLog>
    {
        public void Configure(EntityTypeBuilder<PartnerContactLog> builder)
        {
            builder.ToTable("PartnerContactLogs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ContactType).HasConversion<string>().HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.ContactDirection).HasConversion<string>().HasMaxLength(20).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.Subject).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.MessageBody).HasMaxLength(2000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.RecipientAddress).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.DeliveryStatus).HasMaxLength(50).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.ExternalMessageId).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.Notes).HasMaxLength(500).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => new { x.PartnershipId, x.SentAt }).HasDatabaseName("IX_ContactLog_Partnership");

            builder.HasOne(x => x.SentBy)
                .WithMany()
                .HasForeignKey(x => x.SentById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LinkedDistribution)
                .WithMany()
                .HasForeignKey(x => x.LinkedDistributionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class PartnerNotificationScheduleConfiguration : IEntityTypeConfiguration<PartnerNotificationSchedule>
    {
        public void Configure(EntityTypeBuilder<PartnerNotificationSchedule> builder)
        {
            builder.ToTable("PartnerNotificationSchedules");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TriggerType).HasConversion<string>().HasMaxLength(50).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.Channels).HasMaxLength(200).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TemplateKey).HasMaxLength(100).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => new { x.TriggerDate, x.IsSent }).HasDatabaseName("IX_NotifSchedule_Pending");
        }
    }
}
