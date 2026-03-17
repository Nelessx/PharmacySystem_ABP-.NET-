using System;

namespace PharmacySystem.Medicines;

// Lightweight DTO for category dropdowns
public class CategoryLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}