using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.ChannelType).HasConversion<int>();
        builder.Property(x => x.SmsText).HasMaxLength(1600);
        builder.Property(x => x.PushTitle).HasMaxLength(200);
        builder.Property(x => x.PushBody).HasMaxLength(2000);
    }
}

public class MessageCampaignConfiguration : IEntityTypeConfiguration<MessageCampaign>
{
    public void Configure(EntityTypeBuilder<MessageCampaign> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.TriggerType).HasConversion<int>();
        builder.Property(x => x.TriggerKey).HasMaxLength(80);
        builder.Property(x => x.TargetGender).HasConversion<int>();

        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .HasConstraintName("fk_message_campaigns_message_templates")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TriggerType, x.TriggerKey })
            .HasDatabaseName("idx_message_campaigns_trigger");
    }
}

public class MessageDispatchLogConfiguration : IEntityTypeConfiguration<MessageDispatchLog>
{
    public void Configure(EntityTypeBuilder<MessageDispatchLog> builder)
    {
        builder.Property(x => x.ChannelType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.OccurrenceDate).HasColumnType("date");

        builder.HasOne(x => x.Campaign)
            .WithMany()
            .HasForeignKey(x => x.CampaignId)
            .HasConstraintName("fk_message_dispatch_logs_message_campaigns")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .HasConstraintName("fk_message_dispatch_logs_subjects")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CampaignId, x.SubjectId, x.OccurrenceDate, x.ChannelType })
            .IsUnique()
            .HasDatabaseName("uq_message_dispatch_logs_campaign_subject_date_channel");
    }
}
