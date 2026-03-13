namespace volvcontrol_api.domain.Entities;

public class ServiceRequestDetail
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int EquipmentId { get; set; }
    public int UserId { get; set; }
    public int StatusServiceRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int TypeMaintenanceRecordId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly? ScheduledDate { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    public string AssignedTechnician { get; set; } = string.Empty;
    public DateOnly? CompletionDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public string ClientName { get; set; } = string.Empty;
    public string ClientDocument { get; set; } = string.Empty;
    public string StatusServiceRequestDescription { get; set; } = string.Empty;
    public string TypeMaintenanceRecordDescription { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentQrCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}
