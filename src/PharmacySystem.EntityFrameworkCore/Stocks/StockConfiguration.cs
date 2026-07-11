using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Medicines;
using PharmacySystem.Stocks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PharmacySystem.EntityFrameworkCore.Stocks;

// EF Core configuration for Stock entity
public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        // Main Stocks table
        builder.ToTable(PharmacySystemConsts.DbTablePrefix + "Stocks", PharmacySystemConsts.DbSchema);

        // Configure ABP base properties
        builder.ConfigureByConvention();

        // Required fields and lengths
        builder.Property(x => x.MedicineId)
            .IsRequired();

        builder.Property(x => x.BatchNumber)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitCost)
            .IsRequired()
            .HasPrecision(18, 2);

        // Optional expiry date
        builder.Property(x => x.ExpiryDate);

        // A physical lot is uniquely identified by medicine + batch + expiry.
        // Two deliveries of the same batch number with different expiry dates
        // are genuinely different lots and must be tracked as separate rows.
        builder.HasIndex(x => new { x.MedicineId, x.BatchNumber, x.ExpiryDate }).IsUnique();

        // Restrict delete of a medicine that still has stock rows.
        builder.HasOne<Medicine>()
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}