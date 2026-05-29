using Application.Common.Results;
using Application.Features.ClientVehicleFlows.Dtos;
using Application.Features.ClientVehicleFlows.Requests;

namespace Application.Features.ClientVehicleFlows;

public interface IClientVehicleFlowService
{
    Task<Result<ClientWithVehicleDto>> CreateClientWithVehicleAsync(CreateClientWithVehicleRequest request, CancellationToken cancellationToken = default);
    Task<Result<ClientVehicleDto>> AddVehicleToClientAsync(int personId, AddVehicleToClientRequest request, CancellationToken cancellationToken = default);
    Task<Result<ClientVehicleDto>> TransferVehicleOwnershipAsync(int vehicleId, TransferVehicleOwnershipRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ClientVehicleDto>>> GetClientVehiclesAsync(int personId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ClientServiceOrderSummaryDto>>> GetClientServiceOrdersAsync(int personId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ClientInvoiceSummaryDto>>> GetClientInvoicesAsync(int personId, CancellationToken cancellationToken = default);
}
