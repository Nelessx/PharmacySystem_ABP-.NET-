using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Suppliers;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PharmacySystem.EntityFrameworkCore.Suppliers;

// EF Core configuration for Supplier entity
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable(PharmacySystemConsts.DbTablePrefix + "Suppliers", PharmacySystemConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(128);

        builder.Property(x => x.Phone)
            .HasMaxLength(32);

        builder.Property(x => x.Email)
            .HasMaxLength(128);

        builder.Property(x => x.Address)
            .HasMaxLength(256);

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}