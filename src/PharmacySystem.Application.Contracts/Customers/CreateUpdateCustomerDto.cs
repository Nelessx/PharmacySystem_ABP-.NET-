using System;
using System.ComponentModel.DataAnnotations;
using PharmacySystem.Validation;

namespace PharmacySystem.Customers;

// Input DTO used when creating or updating a customer
public class CreateUpdateCustomerDto
{
    // Customer name is required
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    // Optional phone number
    [StringLength(32)]
    [Phone]
    public string? Phone { get; set; }

    // Optional address
    [StringLength(256)]
    public string? Address { get; set; }

    // Optional gender
    [StringLength(32)]
    public string? Gender { get; set; }

    // Optional date of birth (cannot be in the future)
    [PastOrPresentDate(ErrorMessage = "Date of birth cannot be in the future.")]
    public DateTime? DateOfBirth { get; set; }

    // Optional patient/customer code
    [StringLength(64)]
    public string? PatientCode { get; set; }

    // Active/inactive status
    public bool IsActive { get; set; } = true;
}