using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Purchases;
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

        builder.Property(x => x.InvoiceNumber)
            .HasMaxLength(128);

        builder.Property(x => x.Notes)
            .HasMaxLength(256);

        builder.Property(x => x.TotalAmount)
            .IsRequired();

        builder.Property(x => x.DiscountAmount)
            .IsRequired();

        builder.Property(x => x.NetAmount)
            .IsRequired();



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

            b.Property(x => x.UnitPrice).IsRequired();

            b.Property(x => x.LineTotal).IsRequired();
        });

    }
}