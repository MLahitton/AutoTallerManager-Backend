using Application.Common.Results;
using Application.Features.Audits.Dtos;

namespace Application.Features.Audits;

public interface IAuditService
{
    Task<Result<IReadOnlyList<AuditDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<AuditDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
