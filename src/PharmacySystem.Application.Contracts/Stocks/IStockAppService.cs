using System;
using System.Threading.Tasks;
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
    // Returns stock rows that are below or equal to reorder level
    Task<ListResultDto<LowStockDto>> GetLowStockAsync();

    // Returns stock rows that are expired or expiring within the given number of days
    Task<ListResultDto<ExpiringStockDto>> GetExpiringStockAsync(int days = 30);
}