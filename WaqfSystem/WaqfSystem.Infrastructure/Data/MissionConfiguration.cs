using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Infrastructure.Data
{
    public class InspectionTeamConfiguration : IEntityTypeConfiguration<InspectionTeam>
    {
        public void Configure(EntityTypeBuilder<InspectionTeam> builder)
        {
            builder.ToTable("InspectionTeams");
            builder.HasKey(x => x.Id);
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.TeamName).HasMaxLength(100).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.TeamCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => x.TeamCode).IsUnique();
            builder.HasIndex(x => new { x.GovernorateId, x.IsActive });

            builder.HasOne(x => x.Governorate).WithMany().HasForeignKey(x => x.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Leader).WithMany().HasForeignKey(x => x.LeaderId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InspectionTeamMemberConfiguration : IEntityTypeConfiguration<InspectionTeamMember>
    {
        public void Configure(EntityTypeBuilder<InspectionTeamMember> builder)
        {
            builder.ToTable("InspectionTeamMembers");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TeamId, x.UserId }).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.IsActive });

            builder.HasOne(x => x.Team).WithMany(x => x.Members).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AddedBy).WithMany().HasForeignKey(x => x.AddedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InspectionMissionConfiguration : IEntityTypeConfiguration<InspectionMission>
    {
        public void Configure(EntityTypeBuilder<InspectionMission> builder)
        {
            builder.ToTable("InspectionMissions");
            builder.HasKey(x => x.Id);
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.MissionCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(200).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TargetArea).HasMaxLength(300).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.AssignmentNotes).HasMaxLength(1000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.ReviewNotes).HasMaxLength(1000).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.CancellationReason).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.RejectionReason).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.CorrectionNotes).HasMaxLength(1000).UseCollation("Arabic_CI_AS");

            builder.Property(x => x.MissionType).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Stage).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);

            builder.Property(x => x.AverageDqsScore).HasColumnType("decimal(5,2)");
            builder.Property(x => x.ProgressPercent).HasColumnType("decimal(5,2)");
            builder.Property(x => x.CheckinLat).HasColumnType("decimal(10,7)");
            builder.Property(x => x.CheckinLng).HasColumnType("decimal(10,7)");

            builder.Ignore(x => x.IsOverdue);
            builder.Ignore(x => x.DaysRemaining);

            builder.HasIndex(x => x.MissionCode).IsUnique();
            builder.HasIndex(x => new { x.Stage, x.MissionDate });
            builder.HasIndex(x => new { x.AssignedToUserId, x.Stage });
            builder.HasIndex(x => new { x.GovernorateId, x.MissionDate });
            builder.HasIndex(x => new { x.MissionDate, x.ExpectedCompletionDate });

            builder.HasOne(x => x.Governorate).WithMany().HasForeignKey(x => x.GovernorateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubDistrict).WithMany().HasForeignKey(x => x.SubDistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewerUser).WithMany().HasForeignKey(x => x.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedToTeam).WithMany(x => x.Missions).HasForeignKey(x => x.AssignedToTeamId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ChecklistTemplate).WithMany().HasForeignKey(x => x.ChecklistTemplateId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MissionStageHistoryConfiguration : IEntityTypeConfiguration<MissionStageHistory>
    {
        public void Configure(EntityTypeBuilder<MissionStageHistory> builder)
        {
            builder.ToTable("MissionStageHistory");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromStage).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.ToStage).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Notes).HasMaxLength(500).UseCollation("Arabic_CI_AS");
            builder.Property(x => x.TriggerAction).HasMaxLength(100).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => new { x.MissionId, x.ChangedAt });

            builder.HasOne(x => x.Mission).WithMany(x => x.StageHistory).HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MissionPropertyEntryConfiguration : IEntityTypeConfiguration<MissionPropertyEntry>
    {
        public void Configure(EntityTypeBuilder<MissionPropertyEntry> builder)
        {
            builder.ToTable("MissionPropertyEntries");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LocalId).HasMaxLength(100);
            builder.Property(x => x.EntryStatus).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.DqsAtEntry).HasColumnType("decimal(5,2)");
            builder.Property(x => x.ReviewNotes).HasMaxLength(500).UseCollation("Arabic_CI_AS");

            builder.HasIndex(x => new { x.MissionId, x.EntryStatus });
            builder.HasIndex(x => new { x.MissionId, x.PropertyId }).IsUnique();

            builder.HasOne(x => x.Mission).WithMany(x => x.PropertyEntries).HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EnteredBy).WithMany().HasForeignKey(x => x.EnteredByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MissionChecklistTemplateConfiguration : IEntityTypeConfiguration<MissionChecklistTemplate>
    {
        public void Configure(EntityTypeBuilder<MissionChecklistTemplate> builder)
        {
            builder.ToTable("MissionChecklistTemplates");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemplateName).HasMaxLength(100).UseCollation("Arabic_CI_AS").IsRequired();
            builder.Property(x => x.MissionType).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Items).HasColumnType("nvarchar(max)").IsRequired();

            builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MissionChecklistResultConfiguration : IEntityTypeConfiguration<MissionChecklistResult>
    {
        public void Configure(EntityTypeBuilder<MissionChecklistResult> builder)
        {
            builder.ToTable("MissionChecklistResults");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Results).HasColumnType("nvarchar(max)").IsRequired();
            builder.Property(x => x.CompletionPercent).HasColumnType("decimal(5,2)");

            builder.HasOne(x => x.Mission).WithMany(x => x.ChecklistResults).HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedBy).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
