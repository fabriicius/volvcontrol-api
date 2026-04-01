namespace volvcontrol_api.domain.Model.Response;

public class MaintenanceRecordContextResponse
{
    public IReadOnlyList<MaintenanceRecordListResponse> Maintenances { get; set; } = Array.Empty<MaintenanceRecordListResponse>();
    public IReadOnlyList<StatusMaintenanceRecordResponse> Status { get; set; } = Array.Empty<StatusMaintenanceRecordResponse>();
    public IReadOnlyList<ServiceTypeMaintenanceRecordResponse> ServiceTypes { get; set; } = Array.Empty<ServiceTypeMaintenanceRecordResponse>();
    public MaintenanceRecordServiceRequestContextResponse ServiceRequestContext { get; set; } = new();
}
