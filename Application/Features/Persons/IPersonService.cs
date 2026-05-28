using Application.Common.Results;
using Application.Features.Persons.Dtos;
using Application.Features.Persons.Requests;

namespace Application.Features.Persons;

public interface IPersonService
{
    Task<Result<IReadOnlyList<PersonDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PersonDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PersonDto>> CreateAsync(CreatePersonRequest request, CancellationToken cancellationToken = default);

    Task<Result<PersonDto>> UpdateAsync(int id, UpdatePersonRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
