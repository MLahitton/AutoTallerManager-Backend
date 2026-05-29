using Application.Common.Results;
using Application.Features.VehicleEntryInventories.Dtos;
using Application.Features.VehicleEntryInventories.Requests;

namespace Application.Features.VehicleEntryInventories;

public interface IVehicleEntryInventoryService
{
    Task<Result<IReadOnlyList<VehicleEntryInventoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<VehicleEntryInventoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<VehicleEntryInventoryDto>> CreateAsync(
        CreateVehicleEntryInventoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<VehicleEntryInventoryDto>> UpdateAsync(
        int id,
        UpdateVehicleEntryInventoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
