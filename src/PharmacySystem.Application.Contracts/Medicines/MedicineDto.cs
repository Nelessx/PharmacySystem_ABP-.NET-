using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Medicines;

// Output DTO used when returning medicine data to the UI
public class MedicineDto : FullAuditedEntityDto<Guid>
{
    // Medicine name shown in list/detail pages
    public string Name { get; set; } = string.Empty;

    // Optional generic name
    public string? GenericName { get; set; }

    // Category foreign key
    public Guid CategoryId { get; set; }

    // Category name shown in UI instead of raw GUID
    public string? CategoryName { get; set; }

    // Unit of medicine such as Tablet, Bottle, Strip
    public string? Unit { get; set; }

    // Optional barcode
    public string? Barcode { get; set; }

    // Purchase price from supplier
    public decimal PurchasePrice { get; set; }

    // Selling price to customer
    public decimal SalePrice { get; set; }

    // Stock warning level
    public int ReorderLevel { get; set; }

    // Indicates whether the medicine is active
    public bool IsActive { get; set; }
}