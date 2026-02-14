namespace volvcontrol_api.repository.Entities;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public int StatusEquipmentId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Client Client { get; set; }
    public Category Category { get; set; }
    public StatusEquipment StatusEquipment { get; set; }
}
