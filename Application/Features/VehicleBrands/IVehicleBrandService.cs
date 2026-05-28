using Application.Common.Results;
using Application.Features.VehicleBrands.Dtos;
using Application.Features.VehicleBrands.Requests;

namespace Application.Features.VehicleBrands;

public interface IVehicleBrandService
{
    Task<Result<IReadOnlyList<VehicleBrandDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<VehicleBrandDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<VehicleBrandDto>> CreateAsync(CreateVehicleBrandRequest request, CancellationToken cancellationToken = default);

    Task<Result<VehicleBrandDto>> UpdateAsync(int id, UpdateVehicleBrandRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
