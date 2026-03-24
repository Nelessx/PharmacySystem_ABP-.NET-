namespace PharmacySystem.Dashboard;

// DTO used by dashboard to show quick business statistics
public class DashboardStatsDto
{
    public long TotalMedicines { get; set; }
    public long TotalSuppliers { get; set; }
    public long TotalCustomers { get; set; }
    public long TotalPurchases { get; set; }
    public long TotalSales { get; set; }
    public long TotalStockRows { get; set; }
    public long LowStockCount { get; set; }
    public long ExpiringStockCount { get; set; }
}