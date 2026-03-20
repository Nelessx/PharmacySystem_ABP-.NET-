using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Purchases;

// Output DTO for a single purchase item (line)
public class PurchaseItemDto : EntityDto<Guid>
{
    // Medicine reference
    public Guid MedicineId { get; set; }

    // Optional readable medicine name (for UI)
    public string? MedicineName { get; set; }

    // Batch tracking
    public string? BatchNumber { get; set; }

    // Expiry date
    public DateTime? ExpiryDate { get; set; }

    // Quantity purchased
    public int Quantity { get; set; }

    // Price per unit
    public decimal UnitPrice { get; set; }

    // Calculated line total
    public decimal LineTotal { get; set; }
}