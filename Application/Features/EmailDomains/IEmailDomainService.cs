using Application.Common.Results;
using Application.Features.EmailDomains.Dtos;
using Application.Features.EmailDomains.Requests;

namespace Application.Features.EmailDomains;

public interface IEmailDomainService
{
    Task<Result<IReadOnlyList<EmailDomainDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<EmailDomainDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<EmailDomainDto>> CreateAsync(CreateEmailDomainRequest request, CancellationToken cancellationToken = default);

    Task<Result<EmailDomainDto>> UpdateAsync(int id, UpdateEmailDomainRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
