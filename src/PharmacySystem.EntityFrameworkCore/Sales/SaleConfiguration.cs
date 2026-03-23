using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Sales;
using System;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PharmacySystem.EntityFrameworkCore.Sales;

// EF Core configuration for Sale aggregate root
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        // Main Sales table
        builder.ToTable(PharmacySystemConsts.DbTablePrefix + "Sales", PharmacySystemConsts.DbSchema);

        // Configure ABP base properties
        builder.ConfigureByConvention();

        // Header fields
        builder.Property(x => x.SaleNumber)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Notes)
            .HasMaxLength(256);

        builder.Property(x => x.TotalAmount)
            .IsRequired();

        builder.Property(x => x.DiscountAmount)
            .IsRequired();

        builder.Property(x => x.NetAmount)
            .IsRequired();

        // Configure SaleItem as owned child collection
        builder.OwnsMany<SaleItem>("_items", b =>
        {
            // Child table
            b.ToTable(PharmacySystemConsts.DbTablePrefix + "SaleItems", PharmacySystemConsts.DbSchema);

            // FK back to Sale
            b.WithOwner().HasForeignKey("SaleId");

            // Key for child entity
            b.Property<Guid>("Id");
            b.HasKey("Id");

            // Item fields
            b.Property(x => x.MedicineId).IsRequired();

            b.Property(x => x.BatchNumber)
                .HasMaxLength(64);

            b.Property(x => x.Quantity)
                .IsRequired();

            b.Property(x => x.UnitPrice)
                .IsRequired();

            b.Property(x => x.LineTotal)
                .IsRequired();
        });
    }
}