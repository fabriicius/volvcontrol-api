namespace volvcontrol_api.domain.Model.Request;

public class EquipmentUpdateRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public long EquipmentCategoryId { get; set; }
    public long EquipmentTypeId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateOnly? InstallationDate { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public int StatusEquipmentId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
