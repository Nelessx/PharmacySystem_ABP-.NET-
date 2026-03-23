using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Sales;

// Input DTO for creating/updating sale
public class CreateUpdateSaleDto
{
    // Sale number is required
    [Required]
    [StringLength(64)]
    public string SaleNumber { get; set; } = string.Empty;

    // Optional customer for walk-in sales support
    public Guid? CustomerId { get; set; }

    // Sale date is required logically
    public DateTime SaleDate { get; set; }

    // Optional notes
    [StringLength(256)]
    public string? Notes { get; set; }

    // Discount cannot be negative
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    // At least one item is expected
    [Required]
    public List<CreateUpdateSaleItemDto> Items { get; set; } = new();
}