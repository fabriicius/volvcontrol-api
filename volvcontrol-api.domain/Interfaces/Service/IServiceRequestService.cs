using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IServiceRequestService
{
    Task<ServiceRequestResponse> CreateAsync(ServiceRequestCreateRequest request, CancellationToken cancellationToken = default);
    Task<ServiceRequestDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequestListResponse>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceTypeMaintenanceRecordResponse>> GetServiceTypeMaintenanceRecordsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusServiceRequestResponse>> GetStatusServiceRequestsAsync(CancellationToken cancellationToken = default);
}
