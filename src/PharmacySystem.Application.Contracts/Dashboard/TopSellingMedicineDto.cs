namespace PharmacySystem.Dashboard;

// Represents one medicine in top selling list
public class TopSellingMedicineDto
{
    public string MedicineName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
}