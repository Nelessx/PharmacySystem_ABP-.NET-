namespace PharmacySystem.Dashboard;

// Represents one expiry bucket in dashboard chart
public class ExpiryTimelineDto
{
    public string Label { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}