namespace volvcontrol_api.repository.Entities;

public class Category
{
    public int Id { get; set; }
    public int CategoryTypeId { get; set; }
    public int CategoryBrandId { get; set; }
    public int CategoryModelId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public CategoryType CategoryType { get; set; }
    public CategoryBrand CategoryBrand { get; set; }
    public CategoryModel CategoryModel { get; set; }
}
