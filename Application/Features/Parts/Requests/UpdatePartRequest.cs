namespace Application.Features.Parts.Requests;

public class UpdatePartRequest
{
    public int PartCategoryId { get; set; }
    public int? PartBrandId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}
