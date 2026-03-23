using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Stocks;

// Read-only application service contract for Stock module
public interface IStockAppService :
    IReadOnlyAppService<
        StockDto,
        Guid,
        PagedAndSortedResultRequestDto>
{
}