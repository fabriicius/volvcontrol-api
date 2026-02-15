namespace volvcontrol_api.domain.Model.Request;

public class AddressCreateRequest
{
    public int ClientId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Complement { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}
