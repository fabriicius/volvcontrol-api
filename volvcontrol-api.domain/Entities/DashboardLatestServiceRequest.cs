namespace volvcontrol_api.domain.Entities;

public class DashboardLatestServiceRequest
{
    public string EquipmentName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
}
