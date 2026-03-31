using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Dashboard;

// Application service contract for dashboard
public interface IDashboardAppService : IApplicationService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<List<SalesPurchasesTrendPointDto>> GetSalesPurchasesTrendAsync();
    Task<List<TopSellingMedicineDto>> GetTopSellingMedicinesAsync();
    Task<List<StockByCategoryDto>> GetStockByCategoryAsync();
    Task<List<ExpiryTimelineDto>> GetExpiryTimelineAsync();

}