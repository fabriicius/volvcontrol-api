namespace volvcontrol_api.repository.Entities;

public class PhotosMaintenanceRecord
{
    public int Id { get; set; }
    public int MaintenanceRecordId { get; set; }
    public int PhotosMaintenanceId { get; set; }

    public MaintenanceRecord MaintenanceRecord { get; set; }
    public PhotosMaintenance PhotosMaintenance { get; set; }
}
