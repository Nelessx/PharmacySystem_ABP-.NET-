using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Customers;

// Application service contract for Customer module
public interface ICustomerAppService :
    ICrudAppService<
        CustomerDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCustomerDto>
{
}