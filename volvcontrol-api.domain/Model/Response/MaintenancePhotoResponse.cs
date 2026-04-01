namespace volvcontrol_api.domain.Model.Response;

public class MaintenancePhotoResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoMoment { get; set; } = string.Empty;
}
