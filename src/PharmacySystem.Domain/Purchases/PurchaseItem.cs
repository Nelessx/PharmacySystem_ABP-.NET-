using System;
using Volo.Abp.Domain.Entities;

namespace PharmacySystem.Purchases;

// Child entity inside Purchase aggregate
public class PurchaseItem : Entity<Guid>
{
    // Medicine being purchased
    public Guid MedicineId { get; private set; }

    // Batch number for pharmacy tracking
    public string? BatchNumber { get; private set; }

    // Expiry date of the medicine
    public DateTime? ExpiryDate { get; private set; }

    // Quantity purchased
    public int Quantity { get; private set; }

    // Purchase price per unit
    public decimal UnitPrice { get; private set; }

    // Total amount for this item
    public decimal LineTotal { get; private set; }

    protected PurchaseItem()
    {
    }

    public PurchaseItem(
        Guid id,
        Guid medicineId,
        int quantity,
        decimal unitPrice,
        string? batchNumber = null,
        DateTime? expiryDate = null
    ) : base(id)
    {
        SetMedicine(medicineId);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        SetBatchNumber(batchNumber);
        SetExpiryDate(expiryDate);

        RecalculateLineTotal();
    }

    public void SetMedicine(Guid medicineId)
    {
        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine is required.", nameof(medicineId));
        }

        MedicineId = medicineId;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
        RecalculateLineTotal();
    }

    public void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        UnitPrice = unitPrice;
        RecalculateLineTotal();
    }

    public void SetBatchNumber(string? batchNumber)
    {
        BatchNumber = string.IsNullOrWhiteSpace(batchNumber)
            ? null
            : batchNumber.Trim();
    }

    public void SetExpiryDate(DateTime? expiryDate)
    {
        ExpiryDate = expiryDate;
    }

    private void RecalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice;
    }
}