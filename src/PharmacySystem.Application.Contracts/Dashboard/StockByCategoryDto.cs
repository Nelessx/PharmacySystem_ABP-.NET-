namespace PharmacySystem.Dashboard;

// Represents stock quantity grouped by category
public class StockByCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}