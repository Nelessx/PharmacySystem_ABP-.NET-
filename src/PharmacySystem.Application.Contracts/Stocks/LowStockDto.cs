using System;

namespace PharmacySystem.Stocks;

// DTO used to show medicines whose stock is below reorder level
public class LowStockDto
{
    // Stock row ID
    public Guid StockId { get; set; }

    // Medicine reference
    public Guid MedicineId { get; set; }

    // Readable medicine name
    public string? MedicineName { get; set; }

    // Batch number for that stock row
    public string BatchNumber { get; set; } = string.Empty;

    // Expiry date of the batch
    public DateTime? ExpiryDate { get; set; }

    // Current available quantity
    public int Quantity { get; set; }

    // Medicine reorder level
    public int ReorderLevel { get; set; }
}