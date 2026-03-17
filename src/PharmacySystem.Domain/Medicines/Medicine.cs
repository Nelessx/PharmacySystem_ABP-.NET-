using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PharmacySystem.Medicines;

// Medicine entity represents a product in the pharmacy system
public class Medicine : FullAuditedAggregateRoot<Guid>
{
    // Name of the medicine (e.g., Paracetamol)
    public string Name { get; private set; } = null!;

    // Generic name (e.g., Acetaminophen) - optional
    public string? GenericName { get; private set; }

    // Foreign key → links Medicine to Category
    public Guid CategoryId { get; private set; }

    // Unit of measurement (Tablet, Bottle, Strip, etc.)
    public string? Unit { get; private set; }

    // Barcode for scanning - optional
    public string? Barcode { get; private set; }

    // Price at which medicine is purchased from supplier
    public decimal PurchasePrice { get; private set; }

    // Price at which medicine is sold to customer
    public decimal SalePrice { get; private set; }

    // Minimum stock level before reorder is needed
    public int ReorderLevel { get; private set; }

    // Indicates if the medicine is active or discontinued
    public bool IsActive { get; private set; }

    // Required by EF Core for object materialization
    protected Medicine()
    {
    }

    // Main constructor used when creating a new Medicine
    public Medicine(
        Guid id,
        string name,
        Guid categoryId,
        decimal purchasePrice,
        decimal salePrice,
        string? genericName = null,
        string? unit = null,
        string? barcode = null,
        int reorderLevel = 0,
        bool isActive = true
    ) : base(id)
    {
        SetName(name);
        SetCategory(categoryId);
        SetPrices(purchasePrice, salePrice);
        SetGenericName(genericName);
        SetUnit(unit);
        SetBarcode(barcode);
        SetReorderLevel(reorderLevel);
        SetIsActive(isActive);
    }

    // Sets and validates the medicine name
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Medicine name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    // Sets and validates the category relationship
    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category is required.", nameof(categoryId));
        }

        CategoryId = categoryId;
    }

    // Sets and validates purchase and sale prices
    public void SetPrices(decimal purchasePrice, decimal salePrice)
    {
        if (purchasePrice < 0)
        {
            throw new ArgumentException("Purchase price cannot be negative.");
        }

        if (salePrice < 0)
        {
            throw new ArgumentException("Sale price cannot be negative.");
        }

        // Business rule: selling price should not be less than purchase price
        if (salePrice < purchasePrice)
        {
            throw new ArgumentException("Sale price cannot be less than purchase price.");
        }

        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
    }

    // Sets reorder level with validation
    public void SetReorderLevel(int reorderLevel)
    {
        if (reorderLevel < 0)
        {
            throw new ArgumentException("Reorder level cannot be negative.");
        }

        ReorderLevel = reorderLevel;
    }

    // Sets optional generic name
    public void SetGenericName(string? genericName)
    {
        GenericName = string.IsNullOrWhiteSpace(genericName)
            ? null
            : genericName.Trim();
    }

    // Sets optional unit
    public void SetUnit(string? unit)
    {
        Unit = string.IsNullOrWhiteSpace(unit)
            ? null
            : unit.Trim();
    }

    // Sets optional barcode
    public void SetBarcode(string? barcode)
    {
        Barcode = string.IsNullOrWhiteSpace(barcode)
            ? null
            : barcode.Trim();
    }

    // Enables or disables the medicine
    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}