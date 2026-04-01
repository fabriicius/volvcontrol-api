using volvcontrol_api.domain.Entities;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IDashboardRepository
{
    Task<DashboardSummary> GetSummaryByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
