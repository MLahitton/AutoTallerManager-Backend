using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PartPurchaseDetails.Dtos;
using Application.Features.PartPurchaseDetails.Errors;
using Application.Features.PartPurchaseDetails.Requests;
using Domain.Entities;

namespace Application.Features.PartPurchaseDetails;

public class PartPurchaseDetailService : IPartPurchaseDetailService
{
    private readonly IUnitOfWork _unitOfWork;

    public PartPurchaseDetailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PartPurchaseDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var partPurchaseDetails = await partPurchaseDetailRepository.GetAllAsync(cancellationToken);

        var partPurchaseDetailDtos = partPurchaseDetails
            .OrderBy(x => x.PartPurchaseDetailId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PartPurchaseDetailDto>>.Success(partPurchaseDetailDtos);
    }

    public async Task<Result<PartPurchaseDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var partPurchaseDetail = await partPurchaseDetailRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchaseDetail is null)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.NotFound);
        }

        return Result<PartPurchaseDetailDto>.Success(MapToDto(partPurchaseDetail));
    }

    public async Task<Result<PartPurchaseDetailDto>> CreateAsync(
        CreatePartPurchaseDetailRequest request,
        CancellationToken cancellationToken = default)
    {
        var partPurchaseId = request?.PartPurchaseId ?? 0;
        var partId = request?.PartId ?? 0;
        var quantity = request?.Quantity ?? 0;
        var unitPrice = request?.UnitPrice ?? 0m;

        var validationError = Validate(partPurchaseId, partId, quantity, unitPrice);
        if (validationError is not null)
        {
            return Result<PartPurchaseDetailDto>.Failure(validationError);
        }

        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(partPurchaseId, cancellationToken);

        if (partPurchase is null)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartPurchaseNotFound);
        }

        if (partPurchase.IsCancelled)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.CannotAddDetailToCancelledPurchaseConflict);
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(partId, cancellationToken);

        if (part is null)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartNotFound);
        }

        if (!part.IsActive)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartInactive);
        }

        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var duplicatePartForPurchase = await partPurchaseDetailRepository.ExistsAsync(
            x => x.PartPurchaseId == partPurchaseId && x.PartId == partId,
            cancellationToken);

        if (duplicatePartForPurchase)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.DuplicatePartForPurchaseConflict);
        }

        part.Stock += quantity;

        var existingDetails = await partPurchaseDetailRepository.FindAsync(
            x => x.PartPurchaseId == partPurchaseId,
            cancellationToken);

        var totalAmount = existingDetails.Sum(CalculateSubtotal) + CalculateSubtotal(quantity, unitPrice);
        partPurchase.Total = totalAmount;

        var partPurchaseDetail = new PartPurchaseDetail
        {
            PartPurchaseId = partPurchaseId,
            PartId = partId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        await partPurchaseDetailRepository.AddAsync(partPurchaseDetail, cancellationToken);
        partRepository.Update(part);
        partPurchaseRepository.Update(partPurchase);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartPurchaseDetailDto>.Success(MapToDto(partPurchaseDetail));
    }

    public async Task<Result<PartPurchaseDetailDto>> UpdateAsync(
        int id,
        UpdatePartPurchaseDetailRequest request,
        CancellationToken cancellationToken = default)
    {
        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var partPurchaseDetail = await partPurchaseDetailRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchaseDetail is null)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.NotFound);
        }

        var newPartPurchaseId = request?.PartPurchaseId ?? 0;
        var newPartId = request?.PartId ?? 0;
        var newQuantity = request?.Quantity ?? 0;
        var newUnitPrice = request?.UnitPrice ?? 0m;

        var validationError = Validate(newPartPurchaseId, newPartId, newQuantity, newUnitPrice);
        if (validationError is not null)
        {
            return Result<PartPurchaseDetailDto>.Failure(validationError);
        }

        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var oldPartPurchaseId = partPurchaseDetail.PartPurchaseId;
        var currentPartPurchase = await partPurchaseRepository.GetByIdAsync(oldPartPurchaseId, cancellationToken);

        if (currentPartPurchase is null)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartPurchaseNotFound);
        }

        if (currentPartPurchase.IsCancelled)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.CannotModifyDetailFromCancelledPurchaseConflict);
        }

        PartPurchase newPartPurchase;
        if (newPartPurchaseId == oldPartPurchaseId)
        {
            newPartPurchase = currentPartPurchase;
        }
        else
        {
            var loadedNewPartPurchase = await partPurchaseRepository.GetByIdAsync(newPartPurchaseId, cancellationToken);
            if (loadedNewPartPurchase is null)
            {
                return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartPurchaseNotFound);
            }

            if (loadedNewPartPurchase.IsCancelled)
            {
                return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.CannotMoveDetailToCancelledPurchaseConflict);
            }

            newPartPurchase = loadedNewPartPurchase;
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var oldPart = await partRepository.GetByIdAsync(partPurchaseDetail.PartId, cancellationToken);

        if (oldPart is null)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartNotFound);
        }

        Part newPart;
        if (newPartId == partPurchaseDetail.PartId)
        {
            newPart = oldPart;
        }
        else
        {
            var loadedNewPart = await partRepository.GetByIdAsync(newPartId, cancellationToken);
            if (loadedNewPart is null)
            {
                return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartNotFound);
            }

            newPart = loadedNewPart;
        }

        if (!newPart.IsActive)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.PartInactive);
        }

        var duplicatePartForPurchase = await partPurchaseDetailRepository.ExistsAsync(
            x => x.PartPurchaseId == newPartPurchaseId && x.PartId == newPartId && x.PartPurchaseDetailId != id,
            cancellationToken);

        if (duplicatePartForPurchase)
        {
            return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.DuplicatePartForPurchaseConflict);
        }

        var oldPartId = partPurchaseDetail.PartId;
        var oldQuantity = partPurchaseDetail.Quantity;
        var oldUnitPrice = partPurchaseDetail.UnitPrice;

        if (newPartId == oldPartId)
        {
            var stockAfterAdjustment = oldPart.Stock + (newQuantity - oldQuantity);
            if (stockAfterAdjustment < 0)
            {
                return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.StockWouldBeNegativeInvalid);
            }

            oldPart.Stock = stockAfterAdjustment;
        }
        else
        {
            var oldPartStockAfterRemoval = oldPart.Stock - oldQuantity;
            if (oldPartStockAfterRemoval < 0)
            {
                return Result<PartPurchaseDetailDto>.Failure(PartPurchaseDetailErrors.StockWouldBeNegativeInvalid);
            }

            oldPart.Stock = oldPartStockAfterRemoval;
            newPart.Stock += newQuantity;
        }

        PartPurchase? oldPartPurchase = null;
        if (newPartPurchaseId != oldPartPurchaseId)
        {
            oldPartPurchase = currentPartPurchase;
        }

        if (newPartPurchaseId == oldPartPurchaseId)
        {
            var samePurchaseDetails = await partPurchaseDetailRepository.FindAsync(
                x => x.PartPurchaseId == newPartPurchaseId,
                cancellationToken);

            var totalAmount = samePurchaseDetails.Sum(CalculateSubtotal)
                - CalculateSubtotal(oldQuantity, oldUnitPrice)
                + CalculateSubtotal(newQuantity, newUnitPrice);

            newPartPurchase.Total = totalAmount;
            partPurchaseRepository.Update(newPartPurchase);
        }
        else
        {
            var oldPurchaseDetails = await partPurchaseDetailRepository.FindAsync(
                x => x.PartPurchaseId == oldPartPurchaseId,
                cancellationToken);

            var oldPurchaseTotal = oldPurchaseDetails.Sum(CalculateSubtotal)
                - CalculateSubtotal(oldQuantity, oldUnitPrice);

            oldPartPurchase!.Total = oldPurchaseTotal;
            partPurchaseRepository.Update(oldPartPurchase);

            var newPurchaseDetails = await partPurchaseDetailRepository.FindAsync(
                x => x.PartPurchaseId == newPartPurchaseId,
                cancellationToken);

            var newPurchaseTotal = newPurchaseDetails.Sum(CalculateSubtotal)
                + CalculateSubtotal(newQuantity, newUnitPrice);

            newPartPurchase.Total = newPurchaseTotal;
            partPurchaseRepository.Update(newPartPurchase);
        }

        partPurchaseDetail.PartPurchaseId = newPartPurchaseId;
        partPurchaseDetail.PartId = newPartId;
        partPurchaseDetail.Quantity = newQuantity;
        partPurchaseDetail.UnitPrice = newUnitPrice;

        partPurchaseDetailRepository.Update(partPurchaseDetail);

        partRepository.Update(oldPart);
        if (newPartId != oldPartId)
        {
            partRepository.Update(newPart);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartPurchaseDetailDto>.Success(MapToDto(partPurchaseDetail));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var partPurchaseDetail = await partPurchaseDetailRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchaseDetail is null)
        {
            return Result.Failure(PartPurchaseDetailErrors.NotFound);
        }

        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(partPurchaseDetail.PartPurchaseId, cancellationToken);

        if (partPurchase is null)
        {
            return Result.Failure(PartPurchaseDetailErrors.PartPurchaseNotFound);
        }

        if (partPurchase.IsCancelled)
        {
            return Result.Failure(PartPurchaseDetailErrors.CannotDeleteDetailFromCancelledPurchaseConflict);
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(partPurchaseDetail.PartId, cancellationToken);

        if (part is null)
        {
            return Result.Failure(PartPurchaseDetailErrors.PartNotFound);
        }

        var stockAfterRemoval = part.Stock - partPurchaseDetail.Quantity;
        if (stockAfterRemoval < 0)
        {
            return Result.Failure(PartPurchaseDetailErrors.StockWouldBeNegativeInvalid);
        }

        var purchaseDetails = await partPurchaseDetailRepository.FindAsync(
            x => x.PartPurchaseId == partPurchaseDetail.PartPurchaseId,
            cancellationToken);

        part.Stock = stockAfterRemoval;
        partPurchase.Total = purchaseDetails
            .Where(x => x.PartPurchaseDetailId != id)
            .Sum(CalculateSubtotal);

        partPurchaseDetailRepository.Remove(partPurchaseDetail);
        partRepository.Update(part);
        partPurchaseRepository.Update(partPurchase);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PartPurchaseDetailDto MapToDto(PartPurchaseDetail partPurchaseDetail)
    {
        return new PartPurchaseDetailDto
        {
            PartPurchaseDetailId = partPurchaseDetail.PartPurchaseDetailId,
            PartPurchaseId = partPurchaseDetail.PartPurchaseId,
            PartId = partPurchaseDetail.PartId,
            Quantity = partPurchaseDetail.Quantity,
            UnitPrice = partPurchaseDetail.UnitPrice,
            Subtotal = CalculateSubtotal(partPurchaseDetail)
        };
    }

    private static Error? Validate(int partPurchaseId, int partId, int quantity, decimal unitPrice)
    {
        if (partPurchaseId <= 0)
        {
            return PartPurchaseDetailErrors.PartPurchaseIdInvalid;
        }

        if (partId <= 0)
        {
            return PartPurchaseDetailErrors.PartIdInvalid;
        }

        if (quantity <= 0)
        {
            return PartPurchaseDetailErrors.QuantityInvalid;
        }

        if (unitPrice < 0m)
        {
            return PartPurchaseDetailErrors.UnitPriceInvalid;
        }

        return null;
    }

    private static decimal CalculateSubtotal(PartPurchaseDetail detail)
    {
        return CalculateSubtotal(detail.Quantity, detail.UnitPrice);
    }

    private static decimal CalculateSubtotal(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }
}
