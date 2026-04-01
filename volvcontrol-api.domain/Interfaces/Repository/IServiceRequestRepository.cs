using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IServiceRequestRepository
{
    Task<ServiceRequest> CreateAsync(ServiceRequestCreateRequest request, CancellationToken cancellationToken = default);
    Task<ServiceRequestDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequestListItem>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByRequestNumberAsync(string requestNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceTypeMaintenanceRecord>> GetServiceTypeMaintenanceRecordsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusServiceRequest>> GetStatusServiceRequestsAsync(CancellationToken cancellationToken = default);
}
