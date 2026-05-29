using Application.Common.Results;
using Application.Features.WorkshopIntake.Dtos;
using Application.Features.WorkshopIntake.Requests;

namespace Application.Features.WorkshopIntake;

public interface IWorkshopIntakeService
{
    Task<Result<WorkshopIntakeDto>> CreateServiceOrderAsync(
        int changedByUserId,
        CreateWorkshopIntakeRequest request,
        CancellationToken cancellationToken = default);
}
