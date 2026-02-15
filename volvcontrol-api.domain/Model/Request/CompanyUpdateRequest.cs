namespace volvcontrol_api.domain.Model.Request;

public class CompanyUpdateRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
}
