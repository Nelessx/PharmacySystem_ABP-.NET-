using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace PharmacySystem.Purchases;

// Input DTO for creating/updating purchase
public class CreateUpdatePurchaseDto : IHasConcurrencyStamp
{
    // Purchase number required
    [Required]
    [StringLength(64)]
    public string PurchaseNumber { get; set; } = string.Empty;

    // Supplier required
    [Required]
    public Guid SupplierId { get; set; }

    // Purchase date is required
    [Required]
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

    // Concurrency stamp for ABP optimistic concurrency
    public string ConcurrencyStamp { get; set; } = string.Empty;

    // At least one item is required (MinLength enforces a non-empty purchase;
    // [Required] alone is a no-op on an already-instantiated list).
    [MinLength(1, ErrorMessage = "A purchase must contain at least one item.")]
    public List<CreateUpdatePurchaseItemDto> Items { get; set; } = new();
}