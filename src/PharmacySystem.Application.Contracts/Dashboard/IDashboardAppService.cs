using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace PharmacySystem.Dashboard;

// Application service contract for dashboard
public interface IDashboardAppService : IApplicationService
{
    Task<DashboardStatsDto> GetStatsAsync();
}