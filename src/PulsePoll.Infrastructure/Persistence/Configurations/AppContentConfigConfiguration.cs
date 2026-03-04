using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class AppContentConfigConfiguration : IEntityTypeConfiguration<AppContentConfig>
{
    public void Configure(EntityTypeBuilder<AppContentConfig> builder)
    {
        builder.Property(x => x.ContactTitle).HasMaxLength(200);
        builder.Property(x => x.ContactEmail).HasMaxLength(200);
        builder.Property(x => x.ContactPhone).HasMaxLength(50);
        builder.Property(x => x.ContactWhatsapp).HasMaxLength(50);
    }
}
