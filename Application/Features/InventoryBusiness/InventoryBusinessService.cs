using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.InventoryBusiness.Dtos;
using Application.Features.InventoryBusiness.Errors;
using Application.Features.InventoryBusiness.Requests;
using Domain.Entities;

namespace Application.Features.InventoryBusiness;

public class InventoryBusinessService : IInventoryBusinessService
{
    private const int CancellationReasonMaxLength = 500;
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string CancelAuditActionTypeName = "CANCEL";

    private readonly IUnitOfWork _unitOfWork;

    public InventoryBusinessService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InventoryPurchaseResultDto>> RegisterPurchaseAsync(
        RegisterInventoryPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplierId = request?.SupplierId ?? 0;
        var purchaseDate = request?.PurchaseDate;
        var details = request?.Details;

        var validationError = ValidateRegisterPurchaseInput(supplierId, purchaseDate, details);
        if (validationError is not null)
        {
            return Result<InventoryPurchaseResultDto>.Failure(validationError);
        }

        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return Result<InventoryPurchaseResultDto>.Failure(InventoryBusinessErrors.SupplierNotFound);
        }

        if (!supplier.IsActive)
        {
            return Result<InventoryPurchaseResultDto>.Failure(InventoryBusinessErrors.SupplierInactiveInvalid);
        }

        var detailList = details!.ToList();
        var partIds = detailList
            .Select(x => x.PartId)
            .Distinct()
            .ToList();

        var partRepository = _unitOfWork.Repository<Part>();
        var parts = await partRepository.FindAsync(x => partIds.Contains(x.PartId), cancellationToken);
        var partById = parts.ToDictionary(x => x.PartId, x => x);

        foreach (var detail in detailList)
        {
            if (!partById.TryGetValue(detail.PartId, out var part))
            {
                return Result<InventoryPurchaseResultDto>.Failure(InventoryBusinessErrors.PartNotFound);
            }

            if (!part.IsActive)
            {
                return Result<InventoryPurchaseResultDto>.Failure(InventoryBusinessErrors.PartInactiveInvalid);
            }
        }

        var purchase = new PartPurchase
        {
            SupplierId = supplierId,
            PurchaseDate = purchaseDate ?? DateTime.UtcNow,
            Total = 0m
        };

        var purchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var purchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();

        await purchaseRepository.AddAsync(purchase, cancellationToken);

        var createdDetails = new List<PartPurchaseDetail>(detailList.Count);
        var total = 0m;

        foreach (var detail in detailList)
        {
            var part = partById[detail.PartId];
            part.Stock += detail.Quantity;
            partRepository.Update(part);

            var purchaseDetail = new PartPurchaseDetail
            {
                PartPurchase = purchase,
                PartId = detail.PartId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice
            };

            total += CalculateSubtotal(detail.Quantity, detail.UnitPrice);
            createdDetails.Add(purchaseDetail);
            await purchaseDetailRepository.AddAsync(purchaseDetail, cancellationToken);
        }

        purchase.Total = total;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InventoryPurchaseResultDto>.Success(new InventoryPurchaseResultDto
        {
            PartPurchaseId = purchase.PartPurchaseId,
            SupplierId = purchase.SupplierId,
            PurchaseDate = purchase.PurchaseDate,
            Total = purchase.Total,
            Details = createdDetails
                .OrderBy(x => x.PartPurchaseDetailId)
                .Select(MapPurchaseDetailResult)
                .ToList()
        });
    }

    public async Task<Result<InventoryPurchaseCancellationResultDto>> CancelPurchaseAsync(
        int purchaseId,
        CancelInventoryPurchaseRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (purchaseId <= 0)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.PurchaseIdInvalid);
        }

        if (currentUserId <= 0)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.CurrentUserInvalid);
        }

        var userExists = await _unitOfWork.Repository<User>().ExistsAsync(
            x => x.UserId == currentUserId,
            cancellationToken);

        if (!userExists)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.CurrentUserInvalid);
        }

        var reason = NormalizeOptionalText(request?.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.CancellationReasonRequired);
        }

        if (reason.Length > CancellationReasonMaxLength)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.CancellationReasonTooLong);
        }

        var purchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var purchase = await purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);

        if (purchase is null)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.PurchaseNotFound);
        }

        if (purchase.IsCancelled)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.PurchaseAlreadyCancelledConflict);
        }

        var purchaseDetails = await _unitOfWork.Repository<PartPurchaseDetail>().FindAsync(
            x => x.PartPurchaseId == purchaseId,
            cancellationToken);

        if (purchaseDetails.Count == 0)
        {
            return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.PurchaseHasNoDetails);
        }

        var reversedQuantityByPartId = purchaseDetails
            .GroupBy(x => x.PartId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

        var partIds = reversedQuantityByPartId.Keys.ToArray();
        var partRepository = _unitOfWork.Repository<Part>();
        var parts = await partRepository.FindAsync(
            x => partIds.Contains(x.PartId),
            cancellationToken);
        var partById = parts.ToDictionary(x => x.PartId);

        foreach (var partId in partIds)
        {
            if (!partById.TryGetValue(partId, out var part))
            {
                return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.PartNotFound);
            }

            if (part.Stock - reversedQuantityByPartId[partId] < 0)
            {
                return Result<InventoryPurchaseCancellationResultDto>.Failure(InventoryBusinessErrors.PurchaseCancellationWouldMakeStockNegativeInvalid);
            }
        }

        var affectedParts = new List<InventoryPurchaseCancellationPartDto>();
        foreach (var partId in partIds.OrderBy(x => x))
        {
            var part = partById[partId];
            var reversedQuantity = reversedQuantityByPartId[partId];
            var previousStock = part.Stock;

            part.Stock -= reversedQuantity;
            partRepository.Update(part);

            affectedParts.Add(new InventoryPurchaseCancellationPartDto
            {
                PartId = part.PartId,
                PartName = part.Description,
                PreviousStock = previousStock,
                ReversedQuantity = reversedQuantity,
                NewStock = part.Stock
            });
        }

        var cancelledAt = DateTime.UtcNow;
        purchase.IsCancelled = true;
        purchase.CancelledAt = cancelledAt;
        purchase.CancellationReason = reason;
        purchase.CancelledByUserId = currentUserId;
        purchaseRepository.Update(purchase);

        await TryCreatePurchaseCancellationAuditAsync(
            currentUserId,
            purchase.PartPurchaseId,
            reason,
            affectedParts.Count,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InventoryPurchaseCancellationResultDto>.Success(new InventoryPurchaseCancellationResultDto
        {
            PartPurchaseId = purchase.PartPurchaseId,
            SupplierId = purchase.SupplierId,
            PurchaseDate = purchase.PurchaseDate,
            Total = purchase.Total,
            IsCancelled = purchase.IsCancelled,
            CancelledAt = purchase.CancelledAt,
            CancellationReason = purchase.CancellationReason,
            CancelledByUserId = purchase.CancelledByUserId,
            AffectedParts = affectedParts,
            Message = $"Purchase {purchase.PartPurchaseId} cancelled successfully."
        });
    }

    public async Task<Result<IReadOnlyList<LowStockPartDto>>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _unitOfWork.Repository<Part>().FindAsync(
            x => x.IsActive && x.Stock <= x.MinimumStock,
            cancellationToken);

        var result = parts
            .OrderBy(x => x.Stock)
            .ThenBy(x => x.Description)
            .Select(MapLowStockPart)
            .ToList();

        return Result<IReadOnlyList<LowStockPartDto>>.Success(result);
    }

    public async Task<Result<InventoryAdjustmentResultDto>> AdjustStockAsync(
        int changedByUserId,
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        if (changedByUserId <= 0)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(InventoryBusinessErrors.ChangedByUserIdInvalid);
        }

        var userExists = await _unitOfWork.Repository<User>().ExistsAsync(
            x => x.UserId == changedByUserId,
            cancellationToken);

        if (!userExists)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(InventoryBusinessErrors.ChangedByUserNotFound);
        }

        var partId = request?.PartId ?? 0;
        var adjustmentQuantity = request?.AdjustmentQuantity ?? 0;
        var reason = NormalizeOptionalText(request?.Reason);

        if (partId <= 0)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(InventoryBusinessErrors.PartIdInvalid);
        }

        if (adjustmentQuantity == 0)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(InventoryBusinessErrors.AdjustmentQuantityInvalid);
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(partId, cancellationToken);
        if (part is null)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(InventoryBusinessErrors.PartNotFound);
        }

        var previousStock = part.Stock;
        var newStock = previousStock + adjustmentQuantity;

        if (newStock < 0)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(InventoryBusinessErrors.StockWouldBeNegativeInvalid);
        }

        part.Stock = newStock;
        partRepository.Update(part);

        await TryCreateStockAdjustmentAuditAsync(
            changedByUserId,
            part.PartId,
            previousStock,
            newStock,
            reason,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InventoryAdjustmentResultDto>.Success(new InventoryAdjustmentResultDto
        {
            PartId = part.PartId,
            PreviousStock = previousStock,
            AdjustmentQuantity = adjustmentQuantity,
            NewStock = newStock,
            Reason = reason
        });
    }

    public async Task<Result<InventorySummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _unitOfWork.Repository<Part>().GetAllAsync(cancellationToken);
        var activeParts = parts.Where(x => x.IsActive).ToList();

        var summary = new InventorySummaryDto
        {
            TotalParts = parts.Count,
            ActiveParts = activeParts.Count,
            LowStockParts = activeParts.Count(x => x.Stock <= x.MinimumStock),
            OutOfStockParts = activeParts.Count(x => x.Stock == 0),
            EstimatedInventoryValue = activeParts.Sum(x => x.Stock * x.UnitPrice)
        };

        return Result<InventorySummaryDto>.Success(summary);
    }

    private async Task TryCreateStockAdjustmentAuditAsync(
        int changedByUserId,
        int partId,
        int previousStock,
        int newStock,
        string? reason,
        CancellationToken cancellationToken)
    {
        var auditActionTypes = await _unitOfWork.Repository<AuditActionType>().GetAllAsync(cancellationToken);
        var updateActionType = auditActionTypes.FirstOrDefault(x =>
            x.Name.Equals(UpdateAuditActionTypeName, StringComparison.OrdinalIgnoreCase));

        if (updateActionType is null)
        {
            return;
        }

        var reasonText = string.IsNullOrWhiteSpace(reason) ? "N/A" : reason;
        var description = $"Stock adjusted from {previousStock} to {newStock}. Reason: {reasonText}";

        var audit = new Audit
        {
            UserId = changedByUserId,
            AuditActionTypeId = updateActionType.AuditActionTypeId,
            AffectedEntity = "Parts",
            AffectedRecordId = partId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Audit>().AddAsync(audit, cancellationToken);
    }

    private async Task TryCreatePurchaseCancellationAuditAsync(
        int currentUserId,
        int purchaseId,
        string reason,
        int affectedPartCount,
        CancellationToken cancellationToken)
    {
        var auditActionTypes = await _unitOfWork.Repository<AuditActionType>().GetAllAsync(cancellationToken);
        var updateActionType = auditActionTypes.FirstOrDefault(x =>
            x.Name.Equals(CancelAuditActionTypeName, StringComparison.OrdinalIgnoreCase));

        if (updateActionType is null)
        {
            return;
        }

        var audit = new Audit
        {
            UserId = currentUserId,
            AuditActionTypeId = updateActionType.AuditActionTypeId,
            AffectedEntity = "PartPurchase",
            AffectedRecordId = purchaseId,
            Description = $"Purchase {purchaseId} cancelled. Reason: {reason}. Stock reversed for {affectedPartCount} parts.",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Audit>().AddAsync(audit, cancellationToken);
    }

    private static Error? ValidateRegisterPurchaseInput(
        int supplierId,
        DateTime? purchaseDate,
        IReadOnlyList<RegisterInventoryPurchaseDetailRequest>? details)
    {
        if (supplierId <= 0)
        {
            return InventoryBusinessErrors.SupplierIdInvalid;
        }

        if (purchaseDate.HasValue && purchaseDate.Value == default)
        {
            return InventoryBusinessErrors.PurchaseDateInvalid;
        }

        if (details is null || details.Count == 0)
        {
            return InventoryBusinessErrors.PurchaseDetailsRequired;
        }

        var duplicatedPartId = details
            .GroupBy(x => x.PartId)
            .Any(x => x.Count() > 1);

        if (duplicatedPartId)
        {
            return InventoryBusinessErrors.DuplicatePartInPurchaseConflict;
        }

        foreach (var detail in details)
        {
            if (detail.PartId <= 0)
            {
                return InventoryBusinessErrors.PartIdInvalid;
            }

            if (detail.Quantity <= 0)
            {
                return InventoryBusinessErrors.QuantityInvalid;
            }

            if (detail.UnitPrice < 0m)
            {
                return InventoryBusinessErrors.UnitPriceInvalid;
            }
        }

        return null;
    }

    private static InventoryPurchaseDetailResultDto MapPurchaseDetailResult(PartPurchaseDetail detail)
    {
        return new InventoryPurchaseDetailResultDto
        {
            PartPurchaseDetailId = detail.PartPurchaseDetailId,
            PartId = detail.PartId,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            Subtotal = CalculateSubtotal(detail.Quantity, detail.UnitPrice)
        };
    }

    private static LowStockPartDto MapLowStockPart(Part part)
    {
        return new LowStockPartDto
        {
            PartId = part.PartId,
            PartCategoryId = part.PartCategoryId,
            PartBrandId = part.PartBrandId,
            Code = part.Code,
            Description = part.Description,
            Stock = part.Stock,
            MinimumStock = part.MinimumStock,
            UnitPrice = part.UnitPrice,
            IsActive = part.IsActive
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static decimal CalculateSubtotal(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }
}
