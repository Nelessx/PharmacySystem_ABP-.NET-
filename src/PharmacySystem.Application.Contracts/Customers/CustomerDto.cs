using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Customers;

// Output DTO used when returning customer data to the UI
public class CustomerDto : FullAuditedEntityDto<Guid>
{
    // Customer full name
    public string Name { get; set; } = string.Empty;

    // Optional phone number
    public string? Phone { get; set; }

    // Optional address
    public string? Address { get; set; }

    // Optional gender
    public string? Gender { get; set; }

    // Optional date of birth
    public DateTime? DateOfBirth { get; set; }

    // Optional patient/customer code
    public string? PatientCode { get; set; }

    // Active/inactive status
    public bool IsActive { get; set; }
}