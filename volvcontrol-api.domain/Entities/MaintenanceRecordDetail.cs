namespace volvcontrol_api.domain.Entities;

public class MaintenanceRecordDetail
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

    public string ClientName { get; set; } = string.Empty;
    public string ClientDocument { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentQrCode { get; set; } = string.Empty;
    public string StatusMaintenanceRecordDescription { get; set; } = string.Empty;
    public string ServiceTypeMaintenanceRecordDescription { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public string CreatedByUserEmail { get; set; } = string.Empty;
    public string ServiceRequestNumber { get; set; } = string.Empty;
    public List<ServiceMaintenanceTool> Tools { get; set; } = new();
    public List<MaintenancePhoto> Photos { get; set; } = new();
}
