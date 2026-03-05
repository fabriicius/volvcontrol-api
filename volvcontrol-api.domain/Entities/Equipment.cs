namespace volvcontrol_api.domain.Entities;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public int EquipmentCategoryId { get; set; }
    public int EquipmentTypeId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateOnly? InstallationDate { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public int StatusEquipmentId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public string? CategoryDescription { get; set; }
    public string? TypeDescription { get; set; }
    public string? StatusEquipmentDescription { get; set; }

    public Client? Client { get; set; }
    public StatusEquipment? StatusEquipment { get; set; }
}
