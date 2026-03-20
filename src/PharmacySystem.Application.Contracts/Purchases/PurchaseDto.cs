using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Purchases;

// Output DTO for Purchase (header + items)
public class PurchaseDto : FullAuditedEntityDto<Guid>
{
    // Purchase number shown to user
    public string PurchaseNumber { get; set; } = string.Empty;

    // Supplier reference
    public Guid SupplierId { get; set; }

    // Optional supplier name for UI
    public string? SupplierName { get; set; }

    // Purchase date
    public DateTime PurchaseDate { get; set; }

    // Supplier invoice number
    public string? InvoiceNumber { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Total before discount
    public decimal TotalAmount { get; set; }

    // Discount
    public decimal DiscountAmount { get; set; }

    // Final total
    public decimal NetAmount { get; set; }

    // List of purchase items
    public List<PurchaseItemDto> Items { get; set; } = new();
}