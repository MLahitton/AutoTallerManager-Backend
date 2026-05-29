using Application.Common.Results;
using Application.Features.Parts.Dtos;
using Application.Features.Parts.Requests;

namespace Application.Features.Parts;

public interface IPartService
{
    Task<Result<IReadOnlyList<PartDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PartDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PartDto>> CreateAsync(CreatePartRequest request, CancellationToken cancellationToken = default);

    Task<Result<PartDto>> UpdateAsync(int id, UpdatePartRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
