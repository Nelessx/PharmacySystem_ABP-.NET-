using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Sales;

// Output DTO for a single sale item
public class SaleItemDto : EntityDto<Guid>
{
    // Medicine reference
    public Guid MedicineId { get; set; }

    // Optional readable medicine name for UI
    public string? MedicineName { get; set; }

    // Optional batch number
    public string? BatchNumber { get; set; }

    // Quantity sold
    public int Quantity { get; set; }

    // Price per unit
    public decimal UnitPrice { get; set; }

    // Calculated line total
    public decimal LineTotal { get; set; }
}