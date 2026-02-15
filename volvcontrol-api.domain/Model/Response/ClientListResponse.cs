namespace volvcontrol_api.domain.Model.Response;

public class ClientListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string PlanDescription { get; set; } = string.Empty;
}
