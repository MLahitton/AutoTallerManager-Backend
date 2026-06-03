using Application.Common.Results;
using Application.Features.Staff.Dtos;
using Application.Features.Staff.Requests;

namespace Application.Features.Staff;

public interface IStaffService
{
    Task<Result<StaffUserDto>> RegisterStaffAsync(RegisterStaffRequest request, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result<StaffUserDto>> ActivateUserAsync(int userId, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result<StaffUserDto>> DeactivateUserAsync(int userId, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MechanicSpecialtySummaryDto>>> GetMechanicSpecialtiesAsync(int personId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MechanicSpecialtySummaryDto>>> ReplaceMechanicSpecialtiesAsync(int personId, ReplaceMechanicSpecialtiesRequest request, CancellationToken cancellationToken = default);
}
