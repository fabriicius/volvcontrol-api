namespace volvcontrol_api.domain.Model.Request;

public class ClientUpdateRequest
{
    public int Id { get; set; }
    public int PlansId { get; set; }
    public int StatusClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    /// <summary>Lista de endereços do cliente. Id = 0 para novo; endereços omitidos são removidos.</summary>
    public List<AddressUpdateRequest>? Addresses { get; set; }
}
