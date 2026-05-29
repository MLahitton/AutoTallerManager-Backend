namespace Application.Features.Catalogs.Dtos;

public class PublicRegistrationCatalogsDto
{
    public IReadOnlyList<CatalogItemDto> DocumentTypes { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> Genders { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> Countries { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> Departments { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> Cities { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> StreetTypes { get; set; } = Array.Empty<CatalogItemDto>();
    public IReadOnlyList<CatalogItemDto> Neighborhoods { get; set; } = Array.Empty<CatalogItemDto>();
}
