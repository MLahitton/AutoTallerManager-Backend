using Application.Common.Results;
using Application.Features.VehicleModels.Dtos;
using Application.Features.VehicleModels.Requests;

namespace Application.Features.VehicleModels;

public interface IVehicleModelService
{
    Task<Result<IReadOnlyList<VehicleModelDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<VehicleModelDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<VehicleModelDto>> CreateAsync(CreateVehicleModelRequest request, CancellationToken cancellationToken = default);

    Task<Result<VehicleModelDto>> UpdateAsync(int id, UpdateVehicleModelRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
