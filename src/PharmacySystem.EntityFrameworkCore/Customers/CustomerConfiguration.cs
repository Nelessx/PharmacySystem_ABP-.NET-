using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Customers;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PharmacySystem.EntityFrameworkCore.Customers;

// EF Core configuration for Customer entity
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(PharmacySystemConsts.DbTablePrefix + "Customers", PharmacySystemConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Phone)
            .HasMaxLength(32);

        builder.Property(x => x.Address)
            .HasMaxLength(256);

        builder.Property(x => x.Gender)
            .HasMaxLength(32);

        builder.Property(x => x.PatientCode)
            .HasMaxLength(64);

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}