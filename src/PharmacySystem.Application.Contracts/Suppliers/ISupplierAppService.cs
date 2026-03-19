using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Suppliers;

// Application service contract for Supplier module
public interface ISupplierAppService :
    ICrudAppService<
        SupplierDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateSupplierDto>
{
}