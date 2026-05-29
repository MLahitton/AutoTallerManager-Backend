using Application.Common.Results;
using Application.Features.ServiceExecution.Dtos;
using Application.Features.ServiceExecution.Requests;

namespace Application.Features.ServiceExecution;

public interface IServiceExecutionService
{
    Task<Result<IReadOnlyList<MechanicAssignedServiceDto>>> GetMyAssignedServicesAsync(int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<MechanicActiveOrderDto>>> GetMyActiveOrdersAsync(int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> UpdateWorkPerformedAsync(int orderServiceId, int currentPersonId, IReadOnlyList<string> currentRoles, UpdateWorkPerformedRequest request, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> AssignMechanicAsync(int orderServiceId, AssignMechanicRequest request, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> UnassignMechanicAsync(int orderServiceId, UnassignMechanicRequest request, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> RequestPartAsync(int orderServiceId, int currentPersonId, IReadOnlyList<string> currentRoles, RequestOrderServicePartRequest request, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> ApproveOrderServicePartAsync(int orderServicePartId, int currentPersonId, IReadOnlyList<string> currentRoles, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> RejectOrderServicePartAsync(int orderServicePartId, int currentPersonId, IReadOnlyList<string> currentRoles, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> ChangeOrderServicePartQuantityAsync(int orderServicePartId, int currentPersonId, IReadOnlyList<string> currentRoles, ChangeOrderServicePartQuantityRequest request, CancellationToken cancellationToken = default);
    Task<Result<PendingApprovalDto>> GetClientPendingApprovalsAsync(int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> ApproveOrderServiceAsync(int orderServiceId, int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> RejectOrderServiceAsync(int orderServiceId, int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> ClientApproveOrderServicePartAsync(int orderServicePartId, int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<ServiceExecutionResultDto>> ClientRejectOrderServicePartAsync(int orderServicePartId, int currentPersonId, CancellationToken cancellationToken = default);
}
