namespace PharmacySystem;

public static class PharmacySystemDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    public const string InsufficientStock = "PharmacySystem:Stock:InsufficientStock";
    public const string StockNotFound = "PharmacySystem:Stock:NotFound";
    public const string DiscountExceedsTotal = "PharmacySystem:Transactions:DiscountExceedsTotal";
    public const string MedicineInUse = "PharmacySystem:Medicines:InUse";
    public const string CategoryInUse = "PharmacySystem:Categories:InUse";
}
