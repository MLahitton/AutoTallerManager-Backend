using Application.Common.Results;
using Application.Features.PersonPhones.Dtos;
using Application.Features.PersonPhones.Requests;

namespace Application.Features.PersonPhones;

public interface IPersonPhoneService
{
    Task<Result<IReadOnlyList<PersonPhoneDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PersonPhoneDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PersonPhoneDto>> CreateAsync(CreatePersonPhoneRequest request, CancellationToken cancellationToken = default);

    Task<Result<PersonPhoneDto>> UpdateAsync(int id, UpdatePersonPhoneRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
