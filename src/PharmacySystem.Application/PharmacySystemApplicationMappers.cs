using PharmacySystem.Categories;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
using PharmacySystem.Purchases;
using PharmacySystem.Suppliers;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace PharmacySystem;

[Mapper]
public partial class CategoryToCategoryDtoMapper : MapperBase<Category, CategoryDto>
{
    public override partial CategoryDto Map(Category source);

    public override partial void Map(Category source, CategoryDto destination);
}

[Mapper]
public partial class MedicineToMedicineDtoMapper : MapperBase<Medicine, MedicineDto>
{
    [MapperIgnoreTarget(nameof(MedicineDto.CategoryName))]
    public override partial MedicineDto Map(Medicine source);

    [MapperIgnoreTarget(nameof(MedicineDto.CategoryName))]
    public override partial void Map(Medicine source, MedicineDto destination);
}

[Mapper]
public partial class SupplierToSupplierDtoMapper : MapperBase<Supplier, SupplierDto>
{
    public override partial SupplierDto Map(Supplier source);

    public override partial void Map(Supplier source, SupplierDto destination);
}

[Mapper]
public partial class CustomerToCustomerDtoMapper : MapperBase<Customer, CustomerDto>
{
    public override partial CustomerDto Map(Customer source);

    public override partial void Map(Customer source, CustomerDto destination);
}

// Maps PurchaseItem entity to PurchaseItemDto
[Mapper]
public partial class PurchaseItemToPurchaseItemDtoMapper : MapperBase<PurchaseItem, PurchaseItemDto>
{
    // Maps one PurchaseItem to PurchaseItemDto
    [MapperIgnoreTarget(nameof(PurchaseItemDto.MedicineName))] // MedicineName is not in entity
    public override partial PurchaseItemDto Map(PurchaseItem source);

    // Maps one PurchaseItem into an existing PurchaseItemDto
    [MapperIgnoreTarget(nameof(PurchaseItemDto.MedicineName))] // MedicineName is not in entity
    public override partial void Map(PurchaseItem source, PurchaseItemDto destination);
}

// Maps Purchase entity to PurchaseDto
[Mapper]
public partial class PurchaseToPurchaseDtoMapper : MapperBase<Purchase, PurchaseDto>
{
    // Maps one Purchase to PurchaseDto
    [MapperIgnoreTarget(nameof(PurchaseDto.SupplierName))] // SupplierName is not in entity
    public override partial PurchaseDto Map(Purchase source);

    // Maps one Purchase into an existing PurchaseDto
    [MapperIgnoreTarget(nameof(PurchaseDto.SupplierName))] // SupplierName is not in entity
    public override partial void Map(Purchase source, PurchaseDto destination);
}