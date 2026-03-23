using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Stocks;

// Output DTO for stock list/detail
public class StockDto : FullAuditedEntityDto<Guid>
{
    // Medicine reference
    public Guid MedicineId { get; set; }

    // Readable medicine name for UI
    public string? MedicineName { get; set; }

    // Batch number
    public string BatchNumber { get; set; } = string.Empty;

    // Expiry date
    public DateTime? ExpiryDate { get; set; }

    // Current available quantity
    public int Quantity { get; set; }

    // Unit cost for this batch
    public decimal UnitCost { get; set; }
}