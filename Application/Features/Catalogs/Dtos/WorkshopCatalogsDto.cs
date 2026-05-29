namespace Application.Features.Catalogs.Dtos;

public class WorkshopCatalogsDto
{
    public IReadOnlyList<CatalogItemDto> VehicleTypes { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> VehicleBrands { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> VehicleModels { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> ServiceTypes { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> OrderStatuses { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> InvoiceStatuses { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> PaymentMethods { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> PaymentStatuses { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> CardTypes { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> MechanicSpecialties { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> PartCategories { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> PartBrands { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> AuditActionTypes { get; set; } = Array.Empty<CatalogItemDto>();
}
