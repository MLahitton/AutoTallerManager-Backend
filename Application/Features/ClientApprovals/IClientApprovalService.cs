using Application.Common.Results;
using Application.Features.ClientApprovals.Dtos;

namespace Application.Features.ClientApprovals;

public interface IClientApprovalService
{
    Task<Result<IReadOnlyList<ClientPendingApprovalDto>>> GetPendingApprovalsAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default);

    Task<Result<ClientApprovalActionResultDto>> ApproveOrderServiceAsync(
        int orderServiceId,
        int currentPersonId,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<Result<ClientApprovalActionResultDto>> RejectOrderServiceAsync(
        int orderServiceId,
        int currentPersonId,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<Result<ClientApprovalActionResultDto>> ApproveOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<Result<ClientApprovalActionResultDto>> RejectOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
