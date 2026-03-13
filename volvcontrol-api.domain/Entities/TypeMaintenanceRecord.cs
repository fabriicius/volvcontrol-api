namespace volvcontrol_api.domain.Entities;

public class TypeMaintenanceRecord
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
