using Application.Common.Results;
using Application.Features.ServiceOrderWorkflow.Dtos;
using Application.Features.ServiceOrderWorkflow.Requests;

namespace Application.Features.ServiceOrderWorkflow;

public interface IServiceOrderWorkflowService
{
    Task<Result<ServiceOrderFullDetailDto>> GetFullDetailAsync(
        int serviceOrderId,
        int currentUserId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderWorkflowDto>> ChangeStatusAsync(
        int serviceOrderId,
        int changedByUserId,
        ChangeServiceOrderStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderWorkflowDto>> CancelAsync(
        int serviceOrderId,
        int changedByUserId,
        CancelOrVoidServiceOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderWorkflowDto>> VoidAsync(
        int serviceOrderId,
        int changedByUserId,
        CancelOrVoidServiceOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderWorkflowDto>> CompleteAsync(
        int serviceOrderId,
        int changedByUserId,
        CancellationToken cancellationToken = default);
}
