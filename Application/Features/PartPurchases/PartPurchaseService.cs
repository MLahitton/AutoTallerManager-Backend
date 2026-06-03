using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PartPurchases.Dtos;
using Application.Features.PartPurchases.Errors;
using Application.Features.PartPurchases.Requests;
using Domain.Entities;

namespace Application.Features.PartPurchases;

public class PartPurchaseService : IPartPurchaseService
{
    private const string CreateAuditActionTypeName = "CREATE";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string DeleteAuditActionTypeName = "DELETE";
    private const string PartPurchaseEntityName = "PartPurchase";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public PartPurchaseService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
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
        int currentUserId,
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

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await partPurchaseRepository.AddAsync(partPurchase, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                PartPurchaseEntityName,
                partPurchase.PartPurchaseId,
                $"Part purchase {partPurchase.PartPurchaseId} created.",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<PartPurchaseDto>.Success(MapToDto(partPurchase));
        }, cancellationToken);
    }

    public async Task<Result<PartPurchaseDto>> UpdateAsync(
        int id,
        UpdatePartPurchaseRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchase is null)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.NotFound);
        }

        if (partPurchase.IsCancelled)
        {
            return Result<PartPurchaseDto>.Failure(PartPurchaseErrors.CannotModifyCancelledPurchaseConflict);
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

        await _auditLogger.LogAsync(
            currentUserId,
            UpdateAuditActionTypeName,
            PartPurchaseEntityName,
            partPurchase.PartPurchaseId,
            $"Part purchase {partPurchase.PartPurchaseId} updated.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartPurchaseDto>.Success(MapToDto(partPurchase));
    }

    public async Task<Result> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var partPurchase = await partPurchaseRepository.GetByIdAsync(id, cancellationToken);

        if (partPurchase is null)
        {
            return Result.Failure(PartPurchaseErrors.NotFound);
        }

        if (partPurchase.IsCancelled)
        {
            return Result.Failure(PartPurchaseErrors.CannotDeleteCancelledPurchaseConflict);
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

        await _auditLogger.LogAsync(
            currentUserId,
            DeleteAuditActionTypeName,
            PartPurchaseEntityName,
            partPurchase.PartPurchaseId,
            $"Part purchase {partPurchase.PartPurchaseId} deleted.",
            cancellationToken);

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
