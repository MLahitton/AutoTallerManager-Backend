using Application.Common.Results;
using Application.Features.Catalogs.Dtos;

namespace Application.Features.Catalogs;

public interface ICatalogService
{
    Task<Result<PublicRegistrationCatalogsDto>> GetPublicRegistrationCatalogsAsync(CancellationToken cancellationToken = default);
    Task<Result<WorkshopCatalogsDto>> GetWorkshopCatalogsAsync(CancellationToken cancellationToken = default);
}
