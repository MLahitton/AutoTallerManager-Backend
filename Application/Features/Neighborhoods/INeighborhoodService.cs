using Application.Common.Results;
using Application.Features.Neighborhoods.Dtos;
using Application.Features.Neighborhoods.Requests;

namespace Application.Features.Neighborhoods;

public interface INeighborhoodService
{
    Task<Result<IReadOnlyList<NeighborhoodDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<NeighborhoodDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<NeighborhoodDto>> CreateAsync(CreateNeighborhoodRequest request, CancellationToken cancellationToken = default);

    Task<Result<NeighborhoodDto>> UpdateAsync(int id, UpdateNeighborhoodRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
