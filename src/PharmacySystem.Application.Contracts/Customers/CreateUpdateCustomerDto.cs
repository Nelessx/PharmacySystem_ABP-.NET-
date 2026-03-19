using System;
using System.ComponentModel.DataAnnotations;

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
    public string? Phone { get; set; }

    // Optional address
    [StringLength(256)]
    public string? Address { get; set; }

    // Optional gender
    [StringLength(32)]
    public string? Gender { get; set; }

    // Optional date of birth
    public DateTime? DateOfBirth { get; set; }

    // Optional patient/customer code
    [StringLength(64)]
    public string? PatientCode { get; set; }

    // Active/inactive status
    public bool IsActive { get; set; } = true;
}