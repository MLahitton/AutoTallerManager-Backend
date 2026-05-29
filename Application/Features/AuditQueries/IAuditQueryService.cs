using Application.Common.Results;
using Application.Features.AuditQueries.Dtos;

namespace Application.Features.AuditQueries;

public interface IAuditQueryService
{
    Task<Result<IReadOnlyList<AuditQueryDto>>> GetRecentAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditQueryDto>>> GetByEntityAsync(string? entity, int recordId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditQueryDto>>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
}
