namespace Application.Features.Search.Dtos;

public class SupplierSearchResultDto
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
