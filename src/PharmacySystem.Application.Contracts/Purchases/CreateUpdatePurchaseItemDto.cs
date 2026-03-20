using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Purchases;

// Input DTO for creating/updating purchase items
public class CreateUpdatePurchaseItemDto
{
    // Medicine is required
    [Required]
    public Guid MedicineId { get; set; }

    // Optional batch
    public string? BatchNumber { get; set; }

    // Optional expiry
    public DateTime? ExpiryDate { get; set; }

    // Quantity must be > 0
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Price must be >= 0
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}