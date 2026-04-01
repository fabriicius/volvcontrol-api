using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.service.services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
    }

    public async Task<DashboardSummaryResponse> GetSummaryByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("userId deve ser maior que zero.");

        var summary = await _dashboardRepository.GetSummaryByUserIdAsync(userId, cancellationToken);
        return new DashboardSummaryResponse
        {
            TotalActiveClients = summary.TotalActiveClients,
            TotalEquipments = summary.TotalEquipments,
            TotalOpenServiceRequests = summary.TotalOpenServiceRequests,
            TotalCompletedServiceRequests = summary.TotalCompletedServiceRequests,
            TotalScheduledServiceRequests = summary.TotalScheduledServiceRequests,
            TotalWaitingPartsServiceRequests = summary.TotalWaitingPartsServiceRequests,
            LatestServiceRequests = summary.LatestServiceRequests.Select(i => new DashboardLatestServiceRequestResponse
            {
                EquipmentName = i.EquipmentName,
                ClientName = i.ClientName,
                StatusDescription = i.StatusDescription
            }).ToList()
        };
    }
}
