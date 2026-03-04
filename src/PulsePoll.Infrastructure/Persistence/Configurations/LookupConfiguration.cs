using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Name)
               .IsUnique()
               .HasDatabaseName("uq_cities_name");
    }
}

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(d => d.City)
               .WithMany(c => c.Districts)
               .HasForeignKey(d => d.CityId)
               .HasConstraintName("fk_districts_cities")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.CityId, d.Name })
               .IsUnique()
               .HasDatabaseName("uq_districts_city_id_name");
    }
}

public class ProfessionConfiguration : IEntityTypeConfiguration<Profession>
{
    public void Configure(EntityTypeBuilder<Profession> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(p => p.Name)
               .IsUnique()
               .HasDatabaseName("uq_professions_name");
    }
}

public class EducationLevelConfiguration : IEntityTypeConfiguration<EducationLevel>
{
    public void Configure(EntityTypeBuilder<EducationLevel> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Name)
               .IsUnique()
               .HasDatabaseName("uq_education_levels_name");
    }
}

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(b => b.Name)
               .IsUnique()
               .HasDatabaseName("uq_banks_name");
    }
}

public class SocioeconomicStatusConfiguration : IEntityTypeConfiguration<SocioeconomicStatus>
{
    public void Configure(EntityTypeBuilder<SocioeconomicStatus> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.Name)
               .IsUnique()
               .HasDatabaseName("uq_socioeconomic_statuses_name");
    }
}

public class LSMSocioeconomicStatusConfiguration : IEntityTypeConfiguration<LSMSocioeconomicStatus>
{
    public void Configure(EntityTypeBuilder<LSMSocioeconomicStatus> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.Name)
               .IsUnique()
               .HasDatabaseName("uq_lsm_socioeconomic_statuses_name");
    }
}

public class SpecialCodeConfiguration : IEntityTypeConfiguration<SpecialCode>
{
    public void Configure(EntityTypeBuilder<SpecialCode> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Code).HasMaxLength(20);
        builder.Property(s => s.Description).HasMaxLength(500);
    }
}
