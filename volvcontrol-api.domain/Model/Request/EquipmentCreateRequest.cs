using System.Text.Json.Serialization;

namespace volvcontrol_api.domain.Model.Request;

public class EquipmentCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    [JsonPropertyName("equipmentCategoryId")]
    public int EquipmentCategoryId { get; set; }
    [JsonPropertyName("equipmentTypeId")]
    public int EquipmentTypeId { get; set; }

    // Aliases para compatibilidade com payloads legados/alternativos
    [JsonPropertyName("equipment_category_id")]
    public int EquipmentCategoryIdSnake { set => EquipmentCategoryId = value; }
    [JsonPropertyName("equipament_category_id")]
    public int EquipamentCategoryIdSnake { set => EquipmentCategoryId = value; }
    [JsonPropertyName("equipamentCategoryId")]
    public int EquipamentCategoryId { set => EquipmentCategoryId = value; }
    [JsonPropertyName("equipment_type_id")]
    public int EquipmentTypeIdSnake { set => EquipmentTypeId = value; }
    [JsonPropertyName("equipament_type_id")]
    public int EquipamentTypeIdSnake { set => EquipmentTypeId = value; }
    [JsonPropertyName("equipamentTypeId")]
    public int EquipamentTypeId { set => EquipmentTypeId = value; }

    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateOnly InstallationDate { get; set; }
    public DateOnly WarrantyUntil { get; set; }
    public int StatusEquipmentId { get; set; }
    [JsonPropertyName("imagebase64")]
    public string ImageBase64 { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
