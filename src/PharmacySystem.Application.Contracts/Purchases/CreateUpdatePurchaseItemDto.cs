using System;
using System.ComponentModel.DataAnnotations;
using PharmacySystem.Validation;

namespace PharmacySystem.Purchases;

// Input DTO for creating/updating purchase items
public class CreateUpdatePurchaseItemDto
{
    // Medicine is required
    [Required]
    public Guid MedicineId { get; set; }

    // Batch number is required: stock is tracked per lot.
    [Required]
    [StringLength(64)]
    public string? BatchNumber { get; set; }

    // Optional expiry; when provided it must be in the future (you cannot
    // receive already-expired stock).
    [FutureDate(ErrorMessage = "Expiry date must be in the future.")]
    public DateTime? ExpiryDate { get; set; }

    // Quantity must be between 1 and a sane upper bound
    [Range(1, 100000)]
    public int Quantity { get; set; }

    // Price must be >= 0
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}