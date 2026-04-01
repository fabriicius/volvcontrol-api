namespace volvcontrol_api.domain.Entities;

public class MaintenancePhotoCreate
{
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoMoment { get; set; } = string.Empty; // before | after
}
