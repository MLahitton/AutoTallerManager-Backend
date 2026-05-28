namespace Domain.Entities;

public class PartCategory
{
    public int PartCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
