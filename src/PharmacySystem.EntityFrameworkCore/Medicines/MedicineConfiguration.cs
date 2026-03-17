using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacySystem.Categories;
using PharmacySystem.Medicines;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PharmacySystem.EntityFrameworkCore.Medicines;

// EF Core configuration for Medicine entity
public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable(PharmacySystemConsts.DbTablePrefix + "Medicines", PharmacySystemConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.GenericName)
            .HasMaxLength(128);

        builder.Property(x => x.Unit)
            .HasMaxLength(64);

        builder.Property(x => x.Barcode)
            .HasMaxLength(64);

        builder.Property(x => x.PurchasePrice)
            .IsRequired();

        builder.Property(x => x.SalePrice)
            .IsRequired();

        builder.Property(x => x.ReorderLevel)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Foreign key: many Medicines belong to one Category
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}