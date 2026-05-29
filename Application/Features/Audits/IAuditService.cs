using Application.Common.Results;
using Application.Features.Audits.Dtos;
using Application.Features.Audits.Requests;

namespace Application.Features.Audits;

public interface IAuditService
{
    Task<Result<IReadOnlyList<AuditDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<AuditDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<AuditDto>> CreateAsync(CreateAuditRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuditDto>> UpdateAsync(int id, UpdateAuditRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
