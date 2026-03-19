using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Suppliers;

// Input DTO used when creating or updating a supplier
public class CreateUpdateSupplierDto
{
    // Supplier name is required
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    // Optional contact person name
    [StringLength(128)]
    public string? ContactPerson { get; set; }

    // Optional phone number
    [StringLength(32)]
    public string? Phone { get; set; }

    // Optional email
    [StringLength(128)]
    [EmailAddress]
    public string? Email { get; set; }

    // Optional address
    [StringLength(256)]
    public string? Address { get; set; }

    // Active/inactive status
    public bool IsActive { get; set; } = true;
}