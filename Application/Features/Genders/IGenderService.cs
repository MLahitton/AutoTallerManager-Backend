using Application.Common.Results;
using Application.Features.Genders.Dtos;
using Application.Features.Genders.Requests;

namespace Application.Features.Genders;

public interface IGenderService
{
    Task<Result<IReadOnlyList<GenderDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<GenderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<GenderDto>> CreateAsync(CreateGenderRequest request, CancellationToken cancellationToken = default);

    Task<Result<GenderDto>> UpdateAsync(int id, UpdateGenderRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
