using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class TaxOfficeConfiguration : IEntityTypeConfiguration<TaxOffice>
{
    public void Configure(EntityTypeBuilder<TaxOffice> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Code).HasMaxLength(20);

        builder.HasOne(t => t.City)
               .WithMany()
               .HasForeignKey(t => t.CityId)
               .HasConstraintName("fk_tax_offices_cities")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Name).HasDatabaseName("idx_tax_offices_name");
        builder.HasIndex(t => t.CityId).HasDatabaseName("idx_tax_offices_city_id");
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ShortName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.TaxNumber).IsRequired().HasMaxLength(11);
        builder.Property(c => c.Phone1).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Phone2).HasMaxLength(20);
        builder.Property(c => c.Mobile).HasMaxLength(20);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Address).IsRequired().HasMaxLength(500);
        builder.Property(c => c.LogoUrl).HasMaxLength(512);

        builder.HasOne(c => c.TaxOffice)
               .WithMany()
               .HasForeignKey(c => c.TaxOfficeId)
               .HasConstraintName("fk_customers_tax_offices")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.City)
               .WithMany()
               .HasForeignKey(c => c.CityId)
               .HasConstraintName("fk_customers_cities")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.District)
               .WithMany()
               .HasForeignKey(c => c.DistrictId)
               .HasConstraintName("fk_customers_districts")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("uq_customers_code");
        builder.HasIndex(c => c.TaxNumber).IsUnique().HasDatabaseName("uq_customers_tax_number");
        builder.HasIndex(c => c.Email).HasDatabaseName("idx_customers_email");
    }
}
