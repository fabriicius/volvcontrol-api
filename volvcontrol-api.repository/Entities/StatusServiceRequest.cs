namespace volvcontrol_api.repository.Entities;

public class StatusServiceRequest
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
