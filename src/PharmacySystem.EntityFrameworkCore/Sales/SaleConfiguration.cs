using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
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

        // Sale number is a human-facing document number and must be unique.
        builder.HasIndex(x => x.SaleNumber).IsUnique();

        // Speed up date-range reporting/history queries.
        builder.HasIndex(x => x.SaleDate);

        // Optional customer reference (walk-in sales have none). Restrict delete
        // so a customer with sales history cannot be removed.
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Notes)
            .HasMaxLength(256);

        builder.Property(x => x.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.NetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

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

            b.Property(x => x.ExpiryDate);

            b.Property(x => x.Quantity)
                .IsRequired();

            b.Property(x => x.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            b.Property(x => x.LineTotal)
                .IsRequired()
                .HasPrecision(18, 2);

            // Each sale line references a medicine; restrict delete of a
            // medicine that appears in sales history.
            b.HasOne<Medicine>()
                .WithMany()
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}