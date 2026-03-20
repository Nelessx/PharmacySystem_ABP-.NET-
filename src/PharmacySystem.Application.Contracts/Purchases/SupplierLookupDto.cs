using System;

namespace PharmacySystem.Purchases;

// Lightweight DTO used for Supplier dropdown in Purchase form
public class SupplierLookupDto
{
    // Supplier ID used as the actual selected value
    public Guid Id { get; set; }

    // Supplier name shown to the user in the dropdown
    public string Name { get; set; } = string.Empty;
}