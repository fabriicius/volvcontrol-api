namespace volvcontrol_api.domain.Model;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PositionDescription { get; set; } = string.Empty;
}
