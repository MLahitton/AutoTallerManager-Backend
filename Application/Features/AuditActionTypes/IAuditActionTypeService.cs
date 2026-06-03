using Application.Common.Results;
using Application.Features.AuditActionTypes.Dtos;

namespace Application.Features.AuditActionTypes;

public interface IAuditActionTypeService
{
    Task<Result<IReadOnlyList<AuditActionTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<AuditActionTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
