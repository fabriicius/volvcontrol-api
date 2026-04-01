namespace volvcontrol_api.domain.Entities;

public class MaintenancePhoto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoMoment { get; set; } = string.Empty; // before | after
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
