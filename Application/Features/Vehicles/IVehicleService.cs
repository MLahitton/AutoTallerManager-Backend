using Application.Common.Results;
using Application.Features.Vehicles.Dtos;
using Application.Features.Vehicles.Requests;

namespace Application.Features.Vehicles;

public interface IVehicleService
{
    Task<Result<IReadOnlyList<VehicleDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<VehicleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<VehicleDto>> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default);

    Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
