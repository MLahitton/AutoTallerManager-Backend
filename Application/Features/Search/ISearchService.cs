using Application.Common.Results;
using Application.Features.Search.Dtos;

namespace Application.Features.Search;

public interface ISearchService
{
    Task<Result<IReadOnlyList<ClientSearchResultDto>>> SearchClientsAsync(string? term, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<VehicleSearchResultDto>>> SearchVehiclesAsync(string? term, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ServiceOrderSearchResultDto>>> SearchServiceOrdersAsync(string? term, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InvoiceSearchResultDto>>> SearchInvoicesAsync(string? term, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PartSearchResultDto>>> SearchPartsAsync(string? term, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SupplierSearchResultDto>>> SearchSuppliersAsync(string? term, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<MechanicSearchResultDto>>> SearchMechanicsAsync(string? term, CancellationToken cancellationToken = default);
}
