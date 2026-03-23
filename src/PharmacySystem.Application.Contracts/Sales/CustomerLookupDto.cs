using System;

namespace PharmacySystem.Sales;

// Lightweight DTO for Customer dropdown in Sale form
public class CustomerLookupDto
{
    // Customer ID used as selected value
    public Guid Id { get; set; }

    // Customer name shown in dropdown
    public string Name { get; set; } = string.Empty;
}