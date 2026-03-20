using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Purchases;

// Input DTO for creating/updating purchase
public class CreateUpdatePurchaseDto
{
    // Purchase number required
    [Required]
    [StringLength(64)]
    public string PurchaseNumber { get; set; } = string.Empty;

    // Supplier required
    [Required]
    public Guid SupplierId { get; set; }

    // Purchase date
    public DateTime PurchaseDate { get; set; }

    // Optional invoice number
    [StringLength(128)]
    public string? InvoiceNumber { get; set; }

    // Optional notes
    [StringLength(256)]
    public string? Notes { get; set; }

    // Discount amount
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    // List of items (VERY IMPORTANT)
    [Required]
    public List<CreateUpdatePurchaseItemDto> Items { get; set; } = new();
}