namespace volvcontrol_api.domain.Model.Request;

public class MaintenanceRecordUpdateRequest
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
    public List<string> Tools { get; set; } = new();
    public List<string> BeforeImagesBase64 { get; set; } = new();
    public List<string> AfterImagesBase64 { get; set; } = new();
}
