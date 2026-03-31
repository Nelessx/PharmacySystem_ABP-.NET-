namespace PharmacySystem.Dashboard;

// One point in monthly sales vs purchases trend chart
public class SalesPurchasesTrendPointDto
{
    public string Label { get; set; } = string.Empty;   // e.g. Jan 2026
    public decimal PurchaseAmount { get; set; }
    public decimal SalesAmount { get; set; }
}