namespace volvcontrol_api.repository.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public int ClientId { get; set; }
    public DateOnly? ServiceDate { get; set; }
    public int? ServiceRequestId { get; set; }
    public int StatusMaintenanceRecordId { get; set; }
    public int ServiceTypeMaintenanceRecordId { get; set; }
    public int CreatedByUserId { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string SymptomsReported { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string ServicesPerformed { get; set; } = string.Empty;
    public string ClientSignature { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public DateOnly? NextMaintenanceDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Equipment Equipment { get; set; }
    public Client Client { get; set; }
    public ServiceRequest ServiceRequest { get; set; }
    public StatusMaintenanceRecord StatusMaintenanceRecord { get; set; }
    public ServiceTypeMaintenanceRecord ServiceTypeMaintenanceRecord { get; set; }
    public User CreatedByUser { get; set; }
}
