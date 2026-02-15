namespace volvcontrol_api.domain.Model.Request;

/// <summary>Endereço no update do cliente. Id = 0 indica novo endereço.</summary>
public class AddressUpdateRequest
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Complement { get; set; } = string.Empty;
    /// <summary>Usado apenas para novos endereços (Id = 0).</summary>
    public string CreatedBy { get; set; } = string.Empty;
}
