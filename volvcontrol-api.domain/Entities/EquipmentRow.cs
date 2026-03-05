namespace volvcontrol_api.domain.Entities;

public class EquipmentRow
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int ClientId { get; set; }
    public string? QrCode { get; set; }
    public int EquipmentCategoryId { get; set; }
    public int EquipmentTypeId { get; set; }
    public string? CategoryDescription { get; set; }
    public string? TypeDescription { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DateTime? InstallationDate { get; set; }
    public DateTime? WarrantyUntil { get; set; }
    public int StatusEquipmentId { get; set; }
    public string? StatusEquipmentDescription { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
