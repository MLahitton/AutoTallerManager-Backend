namespace Application.Features.Suppliers.Requests;

public class CreateSupplierRequest
{
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
}
