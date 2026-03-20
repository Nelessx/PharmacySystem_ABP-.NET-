using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Purchases;

// Application service contract for Purchase
public interface IPurchaseAppService :
    ICrudAppService<
        PurchaseDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdatePurchaseDto>
{
    // Returns suppliers for Purchase dropdown
    Task<ListResultDto<SupplierLookupDto>> GetSupplierLookupAsync();

    // Returns medicines for Purchase item dropdown
    Task<ListResultDto<MedicineLookupDto>> GetMedicineLookupAsync();
}