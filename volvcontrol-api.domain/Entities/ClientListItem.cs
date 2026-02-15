namespace volvcontrol_api.domain.Entities;

public class ClientListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string? StatusDescription { get; set; }
    public string? PlanDescription { get; set; }
}
