using Application.Common.Results;
using Application.Features.MechanicSpecialtyAssignments.Dtos;
using Application.Features.MechanicSpecialtyAssignments.Requests;

namespace Application.Features.MechanicSpecialtyAssignments;

public interface IMechanicSpecialtyAssignmentService
{
    Task<Result<IReadOnlyList<MechanicSpecialtyAssignmentDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<MechanicSpecialtyAssignmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<MechanicSpecialtyAssignmentDto>> CreateAsync(CreateMechanicSpecialtyAssignmentRequest request, CancellationToken cancellationToken = default);

    Task<Result<MechanicSpecialtyAssignmentDto>> UpdateAsync(int id, UpdateMechanicSpecialtyAssignmentRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
