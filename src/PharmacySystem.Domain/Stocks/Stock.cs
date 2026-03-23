using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PharmacySystem.Stocks;

// Represents the current available stock for one medicine batch
public class Stock : FullAuditedAggregateRoot<Guid>
{
    // Which medicine this stock belongs to
    public Guid MedicineId { get; private set; }

    // Batch number of the medicine
    public string BatchNumber { get; private set; } = null!;

    // Expiry date of this batch
    public DateTime? ExpiryDate { get; private set; }

    // Current available quantity in stock
    public int Quantity { get; private set; }

    // Purchase/unit cost for this batch
    public decimal UnitCost { get; private set; }

    protected Stock()
    {
    }

    public Stock(
        Guid id,
        Guid medicineId,
        string batchNumber,
        int quantity,
        decimal unitCost,
        DateTime? expiryDate = null
    ) : base(id)
    {
        SetMedicine(medicineId);
        SetBatchNumber(batchNumber);
        SetQuantity(quantity);
        SetUnitCost(unitCost);
        SetExpiryDate(expiryDate);
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

    // Sets batch number
    public void SetBatchNumber(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number cannot be empty.", nameof(batchNumber));
        }

        BatchNumber = batchNumber.Trim();
    }

    // Sets expiry date
    public void SetExpiryDate(DateTime? expiryDate)
    {
        ExpiryDate = expiryDate;
    }

    // Sets current quantity directly
    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(quantity));
        }

        Quantity = quantity;
    }

    // Sets unit cost
    public void SetUnitCost(decimal unitCost)
    {
        if (unitCost < 0)
        {
            throw new ArgumentException("Unit cost cannot be negative.", nameof(unitCost));
        }

        UnitCost = unitCost;
    }

    // Increases stock quantity
    public void Increase(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Increase quantity must be greater than zero.", nameof(quantity));
        }

        Quantity += quantity;
    }

    // Decreases stock quantity
    public void Decrease(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Decrease quantity must be greater than zero.", nameof(quantity));
        }

        if (Quantity < quantity)
        {
            throw new InvalidOperationException("Insufficient stock.");
        }

        Quantity -= quantity;
    }
}