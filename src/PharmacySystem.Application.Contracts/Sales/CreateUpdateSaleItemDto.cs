using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Sales;

// Input DTO for creating/updating sale items
public class CreateUpdateSaleItemDto
{
    // Medicine is required
    [Required]
    public Guid MedicineId { get; set; }

    // Optional batch number
    [StringLength(64)]
    public string? BatchNumber { get; set; }

    // Quantity must be greater than zero
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Unit price cannot be negative
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}