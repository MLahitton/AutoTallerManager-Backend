using Application.Common.Results;
using Application.Features.AdminMechanics.Dtos;

namespace Application.Features.AdminMechanics;

public interface IAdminMechanicsService
{
    Task<Result<IReadOnlyList<AdminMechanicListItemDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<AdminMechanicDetailDto>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default);

    Task<Result<AdminMechanicWorkloadDto>> GetWorkloadAsync(int personId, CancellationToken cancellationToken = default);
}
