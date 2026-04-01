using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
