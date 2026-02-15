namespace volvcontrol_api.domain.Model.Response;

public class ClientResponse
{
    public int Id { get; set; }
    public int PlansId { get; set; }
    public int StatusClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
