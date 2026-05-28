using Application.Common.Results;
using Application.Features.StreetTypes.Dtos;
using Application.Features.StreetTypes.Requests;

namespace Application.Features.StreetTypes;

public interface IStreetTypeService
{
    Task<Result<IReadOnlyList<StreetTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<StreetTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<StreetTypeDto>> CreateAsync(CreateStreetTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result<StreetTypeDto>> UpdateAsync(int id, UpdateStreetTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
