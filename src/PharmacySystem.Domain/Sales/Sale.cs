using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace PharmacySystem.Sales;

// Aggregate root for sale transaction
public class Sale : FullAuditedAggregateRoot<Guid>
{
    // Sale number shown to users
    public string SaleNumber { get; private set; } = null!;

    // Customer reference (optional for walk-in sales)
    public Guid? CustomerId { get; private set; }

    // Date of sale
    public DateTime SaleDate { get; private set; }

    // Optional notes
    public string? Notes { get; private set; }

    // Sum of all sale item totals
    public decimal TotalAmount { get; private set; }

    // Discount amount
    public decimal DiscountAmount { get; private set; }

    // Final amount after discount
    public decimal NetAmount { get; private set; }

    // Child items in this sale
    private readonly List<SaleItem> _items = new();

    // Exposed as read-only for application code; EF should ignore this property
    [NotMapped]
    public IReadOnlyCollection<SaleItem> Items => new ReadOnlyCollection<SaleItem>(_items);

    protected Sale()
    {
    }

    public Sale(
        Guid id,
        string saleNumber,
        DateTime saleDate,
        Guid? customerId = null,
        string? notes = null,
        decimal discountAmount = 0
    ) : base(id)
    {
        SetSaleNumber(saleNumber);
        SetSaleDate(saleDate);
        SetCustomer(customerId);
        SetNotes(notes);
        SetDiscountAmount(discountAmount);

        RecalculateTotals();
    }

    // Sets sale number
    public void SetSaleNumber(string saleNumber)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
        {
            throw new ArgumentException("Sale number cannot be empty.", nameof(saleNumber));
        }

        SaleNumber = saleNumber.Trim();
    }

    // Sets optional customer
    public void SetCustomer(Guid? customerId)
    {
        if (customerId.HasValue && customerId.Value == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty GUID.", nameof(customerId));
        }

        CustomerId = customerId;
    }

    // Sets sale date
    public void SetSaleDate(DateTime saleDate)
    {
        SaleDate = saleDate;
    }

    // Sets optional notes
    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    // Sets discount amount
    public void SetDiscountAmount(decimal discountAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));
        }

        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    // Adds a sale item
    public void AddItem(
        Guid medicineId,
        int quantity,
        decimal unitPrice,
        string? batchNumber = null
    )
    {
        var item = new SaleItem(
            Guid.NewGuid(),
            medicineId,
            quantity,
            unitPrice,
            batchNumber
        );

        _items.Add(item);
        RecalculateTotals();
    }

    // Removes one sale item
    public void RemoveItem(Guid saleItemId)
    {
        var item = _items.FirstOrDefault(x => x.Id == saleItemId);

        if (item == null)
        {
            throw new ArgumentException("Sale item not found.", nameof(saleItemId));
        }

        _items.Remove(item);
        RecalculateTotals();
    }

    // Clears all items
    public void ClearItems()
    {
        _items.Clear();
        RecalculateTotals();
    }

    // Recalculates sale totals
    private void RecalculateTotals()
    {
        TotalAmount = _items.Sum(x => x.LineTotal);
        NetAmount = TotalAmount - DiscountAmount;

        if (NetAmount < 0)
        {
            NetAmount = 0;
        }
    }
}