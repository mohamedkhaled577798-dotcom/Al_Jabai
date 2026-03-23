using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Infrastructure.Data
{
    public class RentContractConfiguration : IEntityTypeConfiguration<RentContract>
    {
        public void Configure(EntityTypeBuilder<RentContract> builder)
        {
            builder.Property(x => x.ContractNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.TenantNameAr).IsRequired().HasMaxLength(200);
            builder.Property(x => x.RentAmount).HasPrecision(18, 2);
            builder.Property(x => x.InsuranceAmount).HasPrecision(18, 2);
            builder.Property(x => x.PenaltyPerDay).HasPrecision(18, 2);

            builder.HasOne(x => x.Property)
                .WithMany()
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Floor)
                .WithMany()
                .HasForeignKey(x => x.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                .WithMany()
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PropertyRevenueConfiguration : IEntityTypeConfiguration<PropertyRevenue>
    {
        public void Configure(EntityTypeBuilder<PropertyRevenue> builder)
        {
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            builder.Property(x => x.PeriodLabel).IsRequired().HasMaxLength(50);
            builder.Property(x => x.RevenueCode).HasMaxLength(30);

            builder.HasOne(x => x.Property)
                .WithMany()
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Batch)
                .WithMany(x => x.Revenues)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasOne(x => x.VarianceApprover)
                .WithMany()
                .HasForeignKey(x => x.VarianceApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CollectedBy)
                .WithMany()
                .HasForeignKey(x => x.CollectedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RevenuePeriodLockConfiguration : IEntityTypeConfiguration<RevenuePeriodLock>
    {
        public void Configure(EntityTypeBuilder<RevenuePeriodLock> builder)
        {
            builder.Property(x => x.PeriodLabel).IsRequired().HasMaxLength(50);
            builder.Property(x => x.ReasonAr).HasMaxLength(300);
            builder.Property(x => x.LockedByRevenueCode).HasMaxLength(30);
            builder.HasIndex(x => new { x.PropertyId, x.FloorId, x.UnitId, x.PeriodLabel }).IsUnique();
        }
    }

    public class RentPaymentScheduleConfiguration : IEntityTypeConfiguration<RentPaymentSchedule>
    {
        public void Configure(EntityTypeBuilder<RentPaymentSchedule> builder)
        {
            builder.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
            builder.Property(x => x.PeriodLabel).IsRequired().HasMaxLength(50);
        }
    }

    public class CollectionBatchConfiguration : IEntityTypeConfiguration<CollectionBatch>
    {
        public void Configure(EntityTypeBuilder<CollectionBatch> builder)
        {
            builder.Property(x => x.BatchCode).IsRequired().HasMaxLength(30);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.Property(x => x.PeriodLabel).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.CollectedBy)
                .WithMany()
                .HasForeignKey(x => x.CollectedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CollectionSmartLogConfiguration : IEntityTypeConfiguration<CollectionSmartLog>
    {
        public void Configure(EntityTypeBuilder<CollectionSmartLog> builder)
        {
            builder.Property(x => x.SuggestionType).IsRequired().HasMaxLength(30);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
