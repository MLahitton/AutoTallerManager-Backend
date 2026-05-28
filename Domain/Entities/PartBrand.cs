namespace Domain.Entities;

public class PartBrand
{
    public int PartBrandId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
