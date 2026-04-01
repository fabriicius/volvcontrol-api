namespace volvcontrol_api.domain.Model.Response;

public class MaintenanceRecordInfoResponse
{
    public int Id { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string ServicesPerformed { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ServiceTypeMaintenanceRecordDescription { get; set; } = string.Empty;
    public string StatusMaintenanceRecordDescription { get; set; } = string.Empty;
    public DateOnly? ServiceDate { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
}
