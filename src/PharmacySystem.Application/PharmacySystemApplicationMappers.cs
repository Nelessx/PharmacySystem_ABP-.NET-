using PharmacySystem.Categories;
using PharmacySystem.Medicines;
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