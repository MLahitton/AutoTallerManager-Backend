using Application.Common.Results;
using Application.Features.AuditActionTypes.Dtos;
using Application.Features.AuditActionTypes.Requests;

namespace Application.Features.AuditActionTypes;

public interface IAuditActionTypeService
{
    Task<Result<IReadOnlyList<AuditActionTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<AuditActionTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<AuditActionTypeDto>> CreateAsync(CreateAuditActionTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuditActionTypeDto>> UpdateAsync(int id, UpdateAuditActionTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
