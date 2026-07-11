using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Sales;

// Input DTO for creating/updating sale items
public class CreateUpdateSaleItemDto
{
    // Medicine is required
    [Required]
    public Guid MedicineId { get; set; }

    // Batch number is required: stock is deducted from a specific lot.
    [Required]
    [StringLength(64)]
    public string? BatchNumber { get; set; }

    // Optional expiry date for batch tracking
    public DateTime? ExpiryDate { get; set; }

    // Quantity must be between 1 and a sane upper bound
    [Range(1, 100000)]
    public int Quantity { get; set; }

    // Unit price cannot be negative
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}