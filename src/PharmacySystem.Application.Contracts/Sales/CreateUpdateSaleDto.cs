using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace PharmacySystem.Sales;

// Input DTO for creating/updating sale
public class CreateUpdateSaleDto : IHasConcurrencyStamp
{
    // Sale number is required
    [Required]
    [StringLength(64)]
    public string SaleNumber { get; set; } = string.Empty;

    // Optional customer for walk-in sales support
    public Guid? CustomerId { get; set; }

    // Sale date is required
    [Required]
    public DateTime SaleDate { get; set; }

    // Optional notes
    [StringLength(256)]
    public string? Notes { get; set; }

    // Discount cannot be negative
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    // At least one item is required ([Required] alone is a no-op on an
    // already-instantiated list, so MinLength enforces a non-empty sale).
    [MinLength(1, ErrorMessage = "A sale must contain at least one item.")]
    public List<CreateUpdateSaleItemDto> Items { get; set; } = new();

    // Concurrency stamp for ABP optimistic concurrency (empty on create)
    public string ConcurrencyStamp { get; set; } = string.Empty;
}