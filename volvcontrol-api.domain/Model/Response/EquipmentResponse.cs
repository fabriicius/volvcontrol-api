using System.Text.Json.Serialization;

namespace volvcontrol_api.domain.Model.Response;

public class EquipmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public int EquipmentCategoryId { get; set; }
    public int EquipmentTypeId { get; set; }
    public string CategoryDescription { get; set; } = string.Empty;
    public string TypeDescription { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateOnly? InstallationDate { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public int StatusEquipmentId { get; set; }
    public string StatusEquipmentDescription { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    [JsonPropertyName("imagebase64")]
    public string ImageBase64 { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
