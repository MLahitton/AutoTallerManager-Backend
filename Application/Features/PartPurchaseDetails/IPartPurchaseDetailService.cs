using Application.Common.Results;
using Application.Features.PartPurchaseDetails.Dtos;
using Application.Features.PartPurchaseDetails.Requests;

namespace Application.Features.PartPurchaseDetails;

public interface IPartPurchaseDetailService
{
    Task<Result<IReadOnlyList<PartPurchaseDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PartPurchaseDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PartPurchaseDetailDto>> CreateAsync(CreatePartPurchaseDetailRequest request, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result<PartPurchaseDetailDto>> UpdateAsync(int id, UpdatePartPurchaseDetailRequest request, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
}
