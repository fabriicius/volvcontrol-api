namespace volvcontrol_api.domain.Model.Response;

public class AddressResponse
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Complement { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
