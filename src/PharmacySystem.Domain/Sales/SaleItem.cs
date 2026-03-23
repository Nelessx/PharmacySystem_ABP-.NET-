using System;
using Volo.Abp.Domain.Entities;

namespace PharmacySystem.Sales;

// Child entity inside Sale aggregate
public class SaleItem : Entity<Guid>
{
    // Medicine being sold
    public Guid MedicineId { get; private set; }

    // Optional batch number for batch-based stock deduction later
    public string? BatchNumber { get; private set; }

    // Quantity sold
    public int Quantity { get; private set; }

    // Selling price per unit
    public decimal UnitPrice { get; private set; }

    // Calculated total for this line
    public decimal LineTotal { get; private set; }

    protected SaleItem()
    {
    }

    public SaleItem(
        Guid id,
        Guid medicineId,
        int quantity,
        decimal unitPrice,
        string? batchNumber = null
    ) : base(id)
    {
        SetMedicine(medicineId);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        SetBatchNumber(batchNumber);

        RecalculateLineTotal();
    }

    // Sets medicine reference
    public void SetMedicine(Guid medicineId)
    {
        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine is required.", nameof(medicineId));
        }

        MedicineId = medicineId;
    }

    // Sets quantity and validates it
    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
        RecalculateLineTotal();
    }

    // Sets unit price and validates it
    public void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        UnitPrice = unitPrice;
        RecalculateLineTotal();
    }

    // Sets optional batch number
    public void SetBatchNumber(string? batchNumber)
    {
        BatchNumber = string.IsNullOrWhiteSpace(batchNumber)
            ? null
            : batchNumber.Trim();
    }

    // Recalculates line total
    private void RecalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice;
    }
}