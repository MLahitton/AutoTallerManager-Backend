namespace Application.Features.Search.Dtos;

public class PartSearchResultDto
{
    public int PartId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}
