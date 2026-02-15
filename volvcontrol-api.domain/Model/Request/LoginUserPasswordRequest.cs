namespace volvcontrol_api.domain.Model.Request;

public class LoginUserPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
