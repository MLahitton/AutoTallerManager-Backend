using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PartPurchases.Dtos;
using Application.Features.PartPurchases.Errors;
using Application.Features.PartPurchases.Requests;
using Domain.Entities;

namespace Application.Features.PartPurchases;

public class PartPurchaseService : IPartPurchaseService
{
    private readonly IUnitOfWork _unitOfWork;

    public PartPurchaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PartPurchaseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchases = await partPurchaseRepository.GetAllAsync(cancellationToken);

        var partPurchaseDtos = partPurchases
            .OrderBy(x => x.PartPurchaseId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PartPurchaseDto>>.Success(partPurchaseDtos);
    }

    public async Task<Result<PartPurchaseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchase is null)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.NotFound);
        }

        return Result<PartPurchaseDto>.Success(MapToDto(partPurchase));
    }

    public async Task<Result<PartPurchaseDto>> CreateAsync(
        CreatePartPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplierId = request?.SupplierId ?? 0;
        var purchaseDate = request?.PurchaseDate;

        var validationError = ValidateCreate(supplierId, purchaseDate);
        if (validationError is not null)
        {
            return Result<PartPurchaseDto>.Failure(validationError);
        }

        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);

        if (supplier is null)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.SupplierNotFound);
        }

        if (!supplier.IsActive)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.SupplierInactive);
        }

        var partPurchase = new PartPurchase
        {
            SupplierId = supplierId,
            PurchaseDate = purchaseDate ?? DateTime.UtcNow,
            Total = 0m
        };

        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        await partPurchaseRepository.AddAsync(partPurchase, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartPurchaseDto>.Success(MapToDto(partPurchase));
    }

    public async Task<Result<PartPurchaseDto>> UpdateAsync(
        int id,
        UpdatePartPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchase is null)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.NotFound);
        }

        var supplierId = request?.SupplierId ?? 0;
        var purchaseDate = request?.PurchaseDate ?? default;

        var validationError = ValidateUpdate(supplierId, purchaseDate);
        if (validationError is not null)
        {
            return Result<PartPurchaseDto>.Failure(validationError);
        }

        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);

        if (supplier is null)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.SupplierNotFound);
        }

        if (!supplier.IsActive)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.SupplierInactive);
        }

        partPurchase.SupplierId = supplierId;
        partPurchase.PurchaseDate = purchaseDate;

        partPurchaseRepository.Update(partPurchase);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartPurchaseDto>.Success(MapToDto(partPurchase));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchase is null)
        {
            return Result.Failure(PartPurchaseErrors.NotFound);
        }

        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var inUse = await partPurchaseDetailRepository.ExistsAsync(
            x => x.PartPurchaseId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(PartPurchaseErrors.InUse);
        }

        partPurchaseRepository.Remove(partPurchase);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PartPurchaseDto MapToDto(PartPurchase partPurchase)
    {
        return new PartPurchaseDto
        {
            PartPurchaseId = partPurchase.PartPurchaseId,
            SupplierId = partPurchase.SupplierId,
            PurchaseDate = partPurchase.PurchaseDate,
            Total = partPurchase.Total
        };
    }

    private static Error? ValidateCreate(int supplierId, DateTime? purchaseDate)
    {
        if (supplierId <= 0)
        {
            return PartPurchaseErrors.SupplierIdInvalid;
        }

        if (purchaseDate.HasValue && purchaseDate.Value == default)
        {
            return PartPurchaseErrors.PurchaseDateInvalid;
        }

        return null;
    }

    private static Error? ValidateUpdate(int supplierId, DateTime purchaseDate)
    {
        if (supplierId <= 0)
        {
            return PartPurchaseErrors.SupplierIdInvalid;
        }

        if (purchaseDate == default)
        {
            return PartPurchaseErrors.PurchaseDateInvalid;
        }

        return null;
    }
}
