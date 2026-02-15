namespace volvcontrol_api.domain.Model.Request;

public class UserCreateRequest
{
    public int CompanyId { get; set; }
    public int UsersPositionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
