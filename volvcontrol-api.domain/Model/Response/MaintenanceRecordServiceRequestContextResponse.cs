namespace volvcontrol_api.domain.Model.Response;

public class MaintenanceRecordServiceRequestContextResponse
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
}
