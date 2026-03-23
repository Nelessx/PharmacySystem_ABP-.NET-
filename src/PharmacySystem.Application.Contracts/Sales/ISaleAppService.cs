using PharmacySystem.Purchases;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Sales;

// Application service contract for Sale module
public interface ISaleAppService :
    ICrudAppService<
        SaleDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateSaleDto>
{
    // Returns customers for Sale dropdown
    Task<ListResultDto<CustomerLookupDto>> GetCustomerLookupAsync();

    // Returns medicines for Sale item dropdown
    Task<ListResultDto<MedicineLookupDto>> GetMedicineLookupAsync();
}