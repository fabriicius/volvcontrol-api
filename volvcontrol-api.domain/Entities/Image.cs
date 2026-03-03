namespace volvcontrol_api.domain.Entities;

public class Image
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
