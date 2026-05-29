using Application.Common.Results;
using Application.Features.MechanicAssignments.Dtos;
using Application.Features.MechanicAssignments.Requests;

namespace Application.Features.MechanicAssignments;

public interface IMechanicAssignmentService
{
    Task<Result<IReadOnlyList<MechanicAssignmentDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<MechanicAssignmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<MechanicAssignmentDto>> CreateAsync(
        CreateMechanicAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<MechanicAssignmentDto>> UpdateAsync(
        int id,
        UpdateMechanicAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
