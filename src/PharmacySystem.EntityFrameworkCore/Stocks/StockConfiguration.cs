using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            .IsRequired();

        // Optional expiry date
        builder.Property(x => x.ExpiryDate);

        // Prevent duplicate stock rows for same medicine + batch
        builder.HasIndex(x => new { x.MedicineId, x.BatchNumber }).IsUnique();
    }
}