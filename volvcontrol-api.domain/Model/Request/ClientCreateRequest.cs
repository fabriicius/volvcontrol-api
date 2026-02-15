namespace volvcontrol_api.domain.Model.Request;

public class ClientCreateRequest
{
    public int CompanyId { get; set; }
    public int PlansId { get; set; }
    public int StatusClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Endereço (1 único no momento da criação)
    public string Street { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Complement { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}
