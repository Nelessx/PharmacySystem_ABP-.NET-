using System;

namespace PharmacySystem.Stocks;

// DTO used to show stock rows that are expired or expiring soon
public class ExpiringStockDto
{
    // Stock row ID
    public Guid StockId { get; set; }

    // Medicine reference
    public Guid MedicineId { get; set; }

    // Readable medicine name
    public string? MedicineName { get; set; }

    // Batch number
    public string BatchNumber { get; set; } = string.Empty;

    // Expiry date of the batch
    public DateTime? ExpiryDate { get; set; }

    // Current available quantity
    public int Quantity { get; set; }

    // Number of days remaining until expiry
    public int DaysToExpire { get; set; }

    // True if already expired
    public bool IsExpired { get; set; }
}