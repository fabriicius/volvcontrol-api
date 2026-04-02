namespace volvcontrol_api.domain.Entities;

public class EquipmentServiceRequestItem
{
    public int EquipmentId { get; set; }
    public int IdServiceRequeist { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientDocument { get; set; } = string.Empty;
    public string StatusServiceRequestDescription { get; set; } = string.Empty;
    public string TypeMaintenanceRecordDescription { get; set; } = string.Empty;
}
