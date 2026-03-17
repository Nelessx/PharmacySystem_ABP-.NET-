using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Medicines;

// Input DTO used when creating or updating a medicine
public class CreateUpdateMedicineDto
{
    // Medicine name is required
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    // Optional generic name
    [StringLength(128)]
    public string? GenericName { get; set; }

    // Required category selection
    [Required]
    public Guid CategoryId { get; set; }

    // Optional unit such as Tablet, Bottle, Strip
    [StringLength(64)]
    public string? Unit { get; set; }

    // Optional barcode
    [StringLength(64)]
    public string? Barcode { get; set; }

    // Purchase price must be zero or greater
    [Range(0, double.MaxValue)]
    public decimal PurchasePrice { get; set; }

    // Sale price must be zero or greater
    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    // Reorder level must be zero or greater
    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; }

    // Active flag
    public bool IsActive { get; set; } = true;
}