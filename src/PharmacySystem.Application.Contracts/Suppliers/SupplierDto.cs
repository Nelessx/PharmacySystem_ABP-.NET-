using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Suppliers;

// Output DTO used when returning supplier data to the UI
public class SupplierDto : FullAuditedEntityDto<Guid>
{
    // Supplier/company name
    public string Name { get; set; } = string.Empty;

    // Contact person - optional
    public string? ContactPerson { get; set; }

    // Phone number - optional
    public string? Phone { get; set; }

    // Email address - optional
    public string? Email { get; set; }

    // Supplier address - optional
    public string? Address { get; set; }

    // Indicates whether the supplier is active
    public bool IsActive { get; set; }
}