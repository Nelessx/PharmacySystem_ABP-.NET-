using System;

namespace PharmacySystem.Sales;

// Lightweight DTO for Medicine dropdown in Sale item rows
public class MedicineLookupDto
{
    // Medicine ID used as selected value
    public Guid Id { get; set; }

    // Medicine name shown in dropdown
    public string Name { get; set; } = string.Empty;
}