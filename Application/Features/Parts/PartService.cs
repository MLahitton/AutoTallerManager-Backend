using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Parts.Dtos;
using Application.Features.Parts.Errors;
using Application.Features.Parts.Requests;
using Domain.Entities;

namespace Application.Features.Parts;

public class PartService : IPartService
{
    private const int CodeMaxLength = 50;
    private const int DescriptionMaxLength = 255;
    private const string CreateAuditActionTypeName = "CREATE";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string DeleteAuditActionTypeName = "DELETE";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public PartService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<IReadOnlyList<PartDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partRepository = _unitOfWork.Repository<Part>();
        var parts = await partRepository.GetAllAsync(cancellationToken);

        var partDtos = parts
            .OrderBy(x => x.PartId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PartDto>>.Success(partDtos);
    }

    public async Task<Result<PartDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(id, cancellationToken);

        if (part is null)
        {
            return Result<PartDto>.Failure(PartErrors.NotFound);
        }

        return Result<PartDto>.Success(MapToDto(part));
    }

    public async Task<Result<PartDto>> CreateAsync(CreatePartRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var partCategoryId = request?.PartCategoryId ?? 0;
        var partBrandId = request?.PartBrandId;
        var code = NormalizeCode(request?.Code);
        var description = NormalizeDescription(request?.Description);
        var stock = request?.Stock ?? 0;
        var minimumStock = request?.MinimumStock ?? 0;
        var unitPrice = request?.UnitPrice ?? 0m;
        var isActive = request?.IsActive ?? true;

        var validationError = Validate(partCategoryId, partBrandId, code, description, stock, minimumStock, unitPrice);
        if (validationError is not null)
        {
            return Result<PartDto>.Failure(validationError);
        }

        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var partCategoryExists = await partCategoryRepository.ExistsAsync(
            x => x.PartCategoryId == partCategoryId,
            cancellationToken);

        if (!partCategoryExists)
        {
            return Result<PartDto>.Failure(PartErrors.PartCategoryNotFound);
        }

        if (partBrandId.HasValue)
        {
            var partBrandRepository = _unitOfWork.Repository<PartBrand>();
            var partBrandExists = await partBrandRepository.ExistsAsync(
                x => x.PartBrandId == partBrandId.Value,
                cancellationToken);

            if (!partBrandExists)
            {
                return Result<PartDto>.Failure(PartErrors.PartBrandNotFound);
            }
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var codeAlreadyExists = await partRepository.ExistsAsync(
            x => x.Code == code,
            cancellationToken);

        if (codeAlreadyExists)
        {
            return Result<PartDto>.Failure(PartErrors.CodeAlreadyExists);
        }

        var part = new Part
        {
            PartCategoryId = partCategoryId,
            PartBrandId = partBrandId,
            Code = code,
            Description = description,
            Stock = stock,
            MinimumStock = minimumStock,
            UnitPrice = unitPrice,
            IsActive = isActive
        };

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await partRepository.AddAsync(part, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                "Parts",
                part.PartId,
                $"Part {part.PartId} created. Name: {part.Description}.",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<PartDto>.Success(MapToDto(part));
        }, cancellationToken);
    }

    public async Task<Result<PartDto>> UpdateAsync(int id, UpdatePartRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(id, cancellationToken);

        if (part is null)
        {
            return Result<PartDto>.Failure(PartErrors.NotFound);
        }

        var partCategoryId = request?.PartCategoryId ?? 0;
        var partBrandId = request?.PartBrandId;
        var code = NormalizeCode(request?.Code);
        var description = NormalizeDescription(request?.Description);
        var stock = request?.Stock ?? 0;
        var minimumStock = request?.MinimumStock ?? 0;
        var unitPrice = request?.UnitPrice ?? 0m;
        var isActive = request?.IsActive ?? false;

        var validationError = Validate(partCategoryId, partBrandId, code, description, stock, minimumStock, unitPrice);
        if (validationError is not null)
        {
            return Result<PartDto>.Failure(validationError);
        }

        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var partCategoryExists = await partCategoryRepository.ExistsAsync(
            x => x.PartCategoryId == partCategoryId,
            cancellationToken);

        if (!partCategoryExists)
        {
            return Result<PartDto>.Failure(PartErrors.PartCategoryNotFound);
        }

        if (partBrandId.HasValue)
        {
            var partBrandRepository = _unitOfWork.Repository<PartBrand>();
            var partBrandExists = await partBrandRepository.ExistsAsync(
                x => x.PartBrandId == partBrandId.Value,
                cancellationToken);

            if (!partBrandExists)
            {
                return Result<PartDto>.Failure(PartErrors.PartBrandNotFound);
            }
        }

        var codeAlreadyExists = await partRepository.ExistsAsync(
            x => x.Code == code && x.PartId != id,
            cancellationToken);

        if (codeAlreadyExists)
        {
            return Result<PartDto>.Failure(PartErrors.CodeAlreadyExists);
        }

        part.PartCategoryId = partCategoryId;
        part.PartBrandId = partBrandId;
        part.Code = code;
        part.Description = description;
        part.Stock = stock;
        part.MinimumStock = minimumStock;
        part.UnitPrice = unitPrice;
        part.IsActive = isActive;

        partRepository.Update(part);
        await _auditLogger.LogAsync(
            currentUserId,
            UpdateAuditActionTypeName,
            "Parts",
            part.PartId,
            $"Part {part.PartId} updated. Name: {part.Description}.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartDto>.Success(MapToDto(part));
    }

    public async Task<Result> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(id, cancellationToken);

        if (part is null)
        {
            return Result.Failure(PartErrors.NotFound);
        }

        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var inUseByOrderServicePart = await orderServicePartRepository.ExistsAsync(
            x => x.PartId == id,
            cancellationToken);

        if (inUseByOrderServicePart)
        {
            return Result.Failure(PartErrors.InUse);
        }

        var partPurchaseDetailRepository = _unitOfWork.Repository<PartPurchaseDetail>();
        var inUseByPartPurchaseDetail = await partPurchaseDetailRepository.ExistsAsync(
            x => x.PartId == id,
            cancellationToken);

        if (inUseByPartPurchaseDetail)
        {
            return Result.Failure(PartErrors.InUse);
        }

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var inUseByInvoiceDetail = await invoiceDetailRepository.ExistsAsync(
            x => x.SourcePartId == id,
            cancellationToken);

        if (inUseByInvoiceDetail)
        {
            return Result.Failure(PartErrors.InUse);
        }

        var partName = part.Description;

        partRepository.Remove(part);
        await _auditLogger.LogAsync(
            currentUserId,
            DeleteAuditActionTypeName,
            "Parts",
            part.PartId,
            $"Part {part.PartId} deleted. Name: {partName}.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PartDto MapToDto(Part part)
    {
        return new PartDto
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

    private static Error? Validate(
        int partCategoryId,
        int? partBrandId,
        string code,
        string description,
        int stock,
        int minimumStock,
        decimal unitPrice)
    {
        if (partCategoryId <= 0)
        {
            return PartErrors.PartCategoryIdInvalid;
        }

        if (partBrandId.HasValue && partBrandId.Value <= 0)
        {
            return PartErrors.PartBrandIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return PartErrors.CodeRequired;
        }

        if (code.Length > CodeMaxLength)
        {
            return PartErrors.CodeTooLong;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return PartErrors.DescriptionRequired;
        }

        if (description.Length > DescriptionMaxLength)
        {
            return PartErrors.DescriptionTooLong;
        }

        if (stock < 0)
        {
            return PartErrors.StockInvalid;
        }

        if (minimumStock < 0)
        {
            return PartErrors.MinimumStockInvalid;
        }

        if (unitPrice < 0m)
        {
            return PartErrors.UnitPriceInvalid;
        }

        return null;
    }

    private static string NormalizeCode(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string NormalizeDescription(string? value)
    {
        return (value ?? string.Empty).Trim();
    }
}
