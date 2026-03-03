namespace volvcontrol_api.domain.Model.Request;

public class ImageUpdateRequest
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
