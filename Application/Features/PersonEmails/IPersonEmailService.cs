using Application.Common.Results;
using Application.Features.PersonEmails.Dtos;
using Application.Features.PersonEmails.Requests;

namespace Application.Features.PersonEmails;

public interface IPersonEmailService
{
    Task<Result<IReadOnlyList<PersonEmailDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PersonEmailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PersonEmailDto>> CreateAsync(CreatePersonEmailRequest request, CancellationToken cancellationToken = default);

    Task<Result<PersonEmailDto>> UpdateAsync(int id, UpdatePersonEmailRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
