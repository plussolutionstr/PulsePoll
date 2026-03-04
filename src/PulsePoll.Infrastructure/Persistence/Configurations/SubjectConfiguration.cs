using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.Property(s => s.PublicId)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.HasIndex(s => s.PublicId)
               .IsUnique()
               .HasDatabaseName("idx_subjects_public_id");

        builder.HasIndex(s => s.Email)
               .IsUnique()
               .HasDatabaseName("idx_subjects_email");

        builder.HasIndex(s => s.PhoneNumber)
               .IsUnique()
               .HasDatabaseName("idx_subjects_phone_number");

        builder.HasIndex(s => s.ReferralCode)
               .IsUnique()
               .HasDatabaseName("idx_subjects_referral_code");

        builder.Property(s => s.IBAN).IsRequired().HasMaxLength(34);
        builder.Property(s => s.IBANFullName).IsRequired().HasMaxLength(200);

        builder.HasOne(s => s.City)
               .WithMany()
               .HasForeignKey(s => s.CityId)
               .HasConstraintName("fk_subjects_cities")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.District)
               .WithMany()
               .HasForeignKey(s => s.DistrictId)
               .HasConstraintName("fk_subjects_districts")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Profession)
               .WithMany()
               .HasForeignKey(s => s.ProfessionId)
               .HasConstraintName("fk_subjects_professions")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.EducationLevel)
               .WithMany()
               .HasForeignKey(s => s.EducationLevelId)
               .HasConstraintName("fk_subjects_education_levels")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.HeadOfFamilyProfession)
               .WithMany()
               .HasForeignKey(s => s.HeadOfFamilyProfessionId)
               .HasConstraintName("fk_subjects_hof_professions")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.HeadOfFamilyEducationLevel)
               .WithMany()
               .HasForeignKey(s => s.HeadOfFamilyEducationLevelId)
               .HasConstraintName("fk_subjects_hof_education_levels")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Bank)
               .WithMany()
               .HasForeignKey(s => s.BankId)
               .HasConstraintName("fk_subjects_banks")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SocioeconomicStatus)
               .WithMany()
               .HasForeignKey(s => s.SocioeconomicStatusId)
               .HasConstraintName("fk_subjects_socioeconomic_statuses")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.LSMSocioeconomicStatus)
               .WithMany()
               .HasForeignKey(s => s.LSMSocioeconomicStatusId)
               .HasConstraintName("fk_subjects_lsm_socioeconomic_statuses")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SpecialCode)
               .WithMany()
               .HasForeignKey(s => s.SpecialCodeId)
               .HasConstraintName("fk_subjects_special_codes")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
