namespace volvcontrol_api.domain.Model.Response;

public class UserResponse
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string PositionDescription { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
