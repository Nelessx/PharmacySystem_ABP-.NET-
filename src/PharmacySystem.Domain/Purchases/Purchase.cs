using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacySystem.Purchases;

// Aggregate root for purchase transaction
public class Purchase : FullAuditedAggregateRoot<Guid>
{
    // Purchase number shown to users
    public string PurchaseNumber { get; private set; } = null!;

    // Supplier reference
    public Guid SupplierId { get; private set; }

    // Date of purchase
    public DateTime PurchaseDate { get; private set; }

    // Supplier invoice number - optional
    public string? InvoiceNumber { get; private set; }

    // Notes - optional
    public string? Notes { get; private set; }

    // Sum of all line totals
    public decimal TotalAmount { get; private set; }

    // Optional discount amount
    public decimal DiscountAmount { get; private set; }

    // Final amount after discount
    public decimal NetAmount { get; private set; }

    // Child items in this purchase
    private readonly List<PurchaseItem> _items = new();
    // Expose items as read-only to application code, but EF should ignore this property
    [NotMapped]
    public IReadOnlyCollection<PurchaseItem> Items => new ReadOnlyCollection<PurchaseItem>(_items);

    protected Purchase()
    {
    }

    public Purchase(
        Guid id,
        string purchaseNumber,
        Guid supplierId,
        DateTime purchaseDate,
        string? invoiceNumber = null,
        string? notes = null,
        decimal discountAmount = 0
    ) : base(id)
    {
        SetPurchaseNumber(purchaseNumber);
        SetSupplier(supplierId);
        SetPurchaseDate(purchaseDate);
        SetInvoiceNumber(invoiceNumber);
        SetNotes(notes);
        SetDiscountAmount(discountAmount);

        RecalculateTotals();
    }

    public void SetPurchaseNumber(string purchaseNumber)
    {
        if (string.IsNullOrWhiteSpace(purchaseNumber))
        {
            throw new ArgumentException("Purchase number cannot be empty.", nameof(purchaseNumber));
        }

        PurchaseNumber = purchaseNumber.Trim();
    }

    public void SetSupplier(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
        {
            throw new ArgumentException("Supplier is required.", nameof(supplierId));
        }

        SupplierId = supplierId;
    }

    public void SetPurchaseDate(DateTime purchaseDate)
    {
        PurchaseDate = purchaseDate;
    }

    public void SetInvoiceNumber(string? invoiceNumber)
    {
        InvoiceNumber = string.IsNullOrWhiteSpace(invoiceNumber)
            ? null
            : invoiceNumber.Trim();
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    public void SetDiscountAmount(decimal discountAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));
        }

        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    public void AddItem(
        Guid medicineId,
        int quantity,
        decimal unitPrice,
        string? batchNumber = null,
        DateTime? expiryDate = null
    )
    {
        var item = new PurchaseItem(
            Guid.NewGuid(),
            medicineId,
            quantity,
            unitPrice,
            batchNumber,
            expiryDate
        );

        _items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid purchaseItemId)
    {
        var item = _items.FirstOrDefault(x => x.Id == purchaseItemId);

        if (item == null)
        {
            throw new ArgumentException("Purchase item not found.", nameof(purchaseItemId));
        }

        _items.Remove(item);
        RecalculateTotals();
    }

    public void ClearItems()
    {
        _items.Clear();
        RecalculateTotals();
    }

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