using System;

namespace PharmacySystem.Purchases;

// Lightweight DTO used for Medicine dropdown in Purchase item rows
public class MedicineLookupDto
{
    // Medicine ID used as the actual selected value
    public Guid Id { get; set; }

    // Medicine name shown to the user in the dropdown
    public string Name { get; set; } = string.Empty;
}