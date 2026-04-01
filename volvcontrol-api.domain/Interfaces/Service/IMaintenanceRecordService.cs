using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IMaintenanceRecordService
{
    Task<MaintenanceRecordDetailResponse> CreateAsync(MaintenanceRecordCreateRequest request, CancellationToken cancellationToken = default);
    Task<MaintenanceRecordDetailResponse?> UpdateAsync(MaintenanceRecordUpdateRequest request, CancellationToken cancellationToken = default);
    Task<MaintenanceRecordDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecordInfoResponse>> GetInfoByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecordListResponse>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecordListResponse>> GetAllByServiceRequestIdAsync(int serviceRequestId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceTypeMaintenanceRecordResponse>> GetServiceTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusMaintenanceRecordResponse>> GetStatusesAsync(CancellationToken cancellationToken = default);
}
