namespace volvcontrol_api.domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string PositionDescription { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public byte Status { get; set; } = 1;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
