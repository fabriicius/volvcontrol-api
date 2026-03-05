namespace volvcontrol_api.domain.Model.Response;

public class ClientDetailResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlansId { get; set; }
    public string PlanDescription { get; set; } = string.Empty;
    public int StatusClientId { get; set; }
    public string StatusClientDescription { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public int EquipmentCount { get; set; }
    public List<ClientEquipmentResponse> Equipments { get; set; } = new();
    public List<AddressResponse> Addresses { get; set; } = new();
}
