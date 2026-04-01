namespace volvcontrol_api.domain.Entities;

public class MaintenanceRecordListItem
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientDocument { get; set; } = string.Empty;
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentQrCode { get; set; } = string.Empty;
    public int StatusMaintenanceRecordId { get; set; }
    public string StatusMaintenanceRecordDescription { get; set; } = string.Empty;
    public int ServiceTypeMaintenanceRecordId { get; set; }
    public string ServiceTypeMaintenanceRecordDescription { get; set; } = string.Empty;
    public DateOnly? ServiceDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
