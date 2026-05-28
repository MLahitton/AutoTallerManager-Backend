using Application.Common.Results;
using Application.Features.VehicleTypes.Dtos;
using Application.Features.VehicleTypes.Requests;

namespace Application.Features.VehicleTypes;

public interface IVehicleTypeService
{
    Task<Result<IReadOnlyList<VehicleTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<VehicleTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<VehicleTypeDto>> CreateAsync(CreateVehicleTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result<VehicleTypeDto>> UpdateAsync(int id, UpdateVehicleTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
