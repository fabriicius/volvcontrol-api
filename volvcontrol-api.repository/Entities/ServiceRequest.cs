namespace volvcontrol_api.repository.Entities;

public class ServiceRequest
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int EquipmentId { get; set; }
    public int UserId { get; set; }
    public int StatusMaintenanceRecordId { get; set; }
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

    public Client Client { get; set; }
    public Equipment Equipment { get; set; }
    public User User { get; set; }
    public StatusMaintenanceRecord StatusMaintenanceRecord { get; set; }
    public TypeMaintenanceRecord TypeMaintenanceRecord { get; set; }
}
