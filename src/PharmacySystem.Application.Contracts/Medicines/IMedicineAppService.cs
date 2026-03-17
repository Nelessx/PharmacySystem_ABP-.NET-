using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Medicines;

// Application service contract for Medicine module
public interface IMedicineAppService :
    ICrudAppService<
        MedicineDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateMedicineDto>
{
    // Used by Angular dropdown to load categories
    Task<ListResultDto<CategoryLookupDto>> GetCategoryLookupAsync();
}