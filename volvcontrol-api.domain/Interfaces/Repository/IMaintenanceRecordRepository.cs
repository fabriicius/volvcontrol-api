using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IMaintenanceRecordRepository
{
    Task<MaintenanceRecord> CreateAsync(MaintenanceRecordCreateRequest request, IReadOnlyList<MaintenancePhotoCreate> photos, CancellationToken cancellationToken = default);
    Task<MaintenanceRecord?> UpdateAsync(MaintenanceRecordUpdateRequest request, IReadOnlyList<MaintenancePhotoCreate> photos, CancellationToken cancellationToken = default);
    Task<MaintenanceRecordDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecordInfoItem>> GetInfoByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecordListItem>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecordListItem>> GetAllByServiceRequestIdAsync(int serviceRequestId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceTypeMaintenanceRecord>> GetServiceTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusMaintenanceRecord>> GetStatusesAsync(CancellationToken cancellationToken = default);
}
