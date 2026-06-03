using Application.Common.Results;
using Application.Features.InventoryBusiness.Dtos;
using Application.Features.InventoryBusiness.Requests;

namespace Application.Features.InventoryBusiness;

public interface IInventoryBusinessService
{
    Task<Result<InventoryPurchaseResultDto>> RegisterPurchaseAsync(RegisterInventoryPurchaseRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<Result<InventoryPurchaseCancellationResultDto>> CancelPurchaseAsync(int purchaseId, CancelInventoryPurchaseRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<LowStockPartDto>>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<Result<InventoryAdjustmentResultDto>> AdjustStockAsync(int changedByUserId, AdjustStockRequest request, CancellationToken cancellationToken = default);
    Task<Result<InventorySummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
