using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Medicines;
using PharmacySystem.Purchases;
using PharmacySystem.Suppliers;
using System;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PharmacySystem.EntityFrameworkCore.Purchases;

// EF Core configuration for Purchase aggregate
public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable(PharmacySystemConsts.DbTablePrefix + "Purchases", PharmacySystemConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.PurchaseNumber)
            .IsRequired()
            .HasMaxLength(64);

        // Purchase number is a human-facing document number and must be unique.
        builder.HasIndex(x => x.PurchaseNumber).IsUnique();

        // Speed up date-range reporting/history queries.
        builder.HasIndex(x => x.PurchaseDate);

        // Restrict delete of a supplier that has purchase history.
        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.InvoiceNumber)
            .HasMaxLength(128);

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



        builder.OwnsMany<PurchaseItem>("_items", b =>
        {
            b.ToTable(PharmacySystemConsts.DbTablePrefix + "PurchaseItems", PharmacySystemConsts.DbSchema);

            b.WithOwner().HasForeignKey("PurchaseId");

            b.Property<Guid>("Id");
            b.HasKey("Id");

            b.Property(x => x.MedicineId).IsRequired();

            b.Property(x => x.BatchNumber).HasMaxLength(64);

            b.Property(x => x.ExpiryDate);

            b.Property(x => x.Quantity).IsRequired();

            b.Property(x => x.UnitPrice).IsRequired().HasPrecision(18, 2);

            b.Property(x => x.LineTotal).IsRequired().HasPrecision(18, 2);

            // Each purchase line references a medicine; restrict delete of a
            // medicine that appears in purchase history.
            b.HasOne<Medicine>()
                .WithMany()
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}