using PharmacySystem.Categories;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
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