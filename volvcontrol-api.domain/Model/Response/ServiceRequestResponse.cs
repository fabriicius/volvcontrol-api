namespace volvcontrol_api.domain.Model.Response;

public class ServiceRequestResponse
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
}
