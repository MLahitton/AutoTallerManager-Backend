using Application.Common.Results;
using Application.Features.VehicleOwnerHistories.Dtos;
using Application.Features.VehicleOwnerHistories.Requests;

namespace Application.Features.VehicleOwnerHistories;

public interface IVehicleOwnerHistoryService
{
    Task<Result<IReadOnlyList<VehicleOwnerHistoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<VehicleOwnerHistoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<VehicleOwnerHistoryDto>> CreateAsync(CreateVehicleOwnerHistoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<VehicleOwnerHistoryDto>> UpdateAsync(int id, UpdateVehicleOwnerHistoryRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
