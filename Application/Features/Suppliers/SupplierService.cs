using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Suppliers.Dtos;
using Application.Features.Suppliers.Errors;
using Application.Features.Suppliers.Requests;
using Domain.Entities;

namespace Application.Features.Suppliers;

public class SupplierService : ISupplierService
{
    private const int NameMaxLength = 120;
    private const int TaxIdMaxLength = 30;
    private const int PhoneMaxLength = 30;
    private const int EmailMaxLength = 120;
    private const string CreateAuditActionTypeName = "CREATE";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string DeleteAuditActionTypeName = "DELETE";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public SupplierService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<IReadOnlyList<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var suppliers = await supplierRepository.GetAllAsync(cancellationToken);

        var supplierDtos = suppliers
            .OrderBy(x => x.SupplierId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<SupplierDto>>.Success(supplierDtos);
    }

    public async Task<Result<SupplierDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var supplier = await supplierRepository.GetByIdAsync(id, cancellationToken);

        if (supplier is null)
        {
            return Result<SupplierDto>.Failure(SupplierErrors.NotFound);
        }

        return Result<SupplierDto>.Success(MapToDto(supplier));
    }

    public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var name = NormalizeRequiredText(request?.Name);
        var taxId = NormalizeOptionalText(request?.TaxId);
        var phone = NormalizeOptionalText(request?.Phone);
        var email = NormalizeEmail(request?.Email);
        var isActive = request?.IsActive ?? true;

        var validationError = Validate(name, taxId, phone, email);
        if (validationError is not null)
        {
            return Result<SupplierDto>.Failure(validationError);
        }

        var supplierRepository = _unitOfWork.Repository<Supplier>();

        if (!string.IsNullOrWhiteSpace(taxId))
        {
            var taxIdAlreadyExists = await supplierRepository.ExistsAsync(
                x => x.TaxId == taxId,
                cancellationToken);

            if (taxIdAlreadyExists)
            {
                return Result<SupplierDto>.Failure(SupplierErrors.TaxIdAlreadyExists);
            }
        }

        var supplier = new Supplier
        {
            Name = name,
            TaxId = taxId,
            Phone = phone,
            Email = email,
            IsActive = isActive
        };

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await supplierRepository.AddAsync(supplier, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                "Suppliers",
                supplier.SupplierId,
                $"Supplier {supplier.SupplierId} created. Name: {supplier.Name}.",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<SupplierDto>.Success(MapToDto(supplier));
        }, cancellationToken);
    }

    public async Task<Result<SupplierDto>> UpdateAsync(int id, UpdateSupplierRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var supplier = await supplierRepository.GetByIdAsync(id, cancellationToken);

        if (supplier is null)
        {
            return Result<SupplierDto>.Failure(SupplierErrors.NotFound);
        }

        var name = NormalizeRequiredText(request?.Name);
        var taxId = NormalizeOptionalText(request?.TaxId);
        var phone = NormalizeOptionalText(request?.Phone);
        var email = NormalizeEmail(request?.Email);
        var isActive = request?.IsActive ?? false;

        var validationError = Validate(name, taxId, phone, email);
        if (validationError is not null)
        {
            return Result<SupplierDto>.Failure(validationError);
        }

        if (!string.IsNullOrWhiteSpace(taxId))
        {
            var taxIdAlreadyExists = await supplierRepository.ExistsAsync(
                x => x.TaxId == taxId && x.SupplierId != id,
                cancellationToken);

            if (taxIdAlreadyExists)
            {
                return Result<SupplierDto>.Failure(SupplierErrors.TaxIdAlreadyExists);
            }
        }

        supplier.Name = name;
        supplier.TaxId = taxId;
        supplier.Phone = phone;
        supplier.Email = email;
        supplier.IsActive = isActive;

        supplierRepository.Update(supplier);
        await _auditLogger.LogAsync(
            currentUserId,
            UpdateAuditActionTypeName,
            "Suppliers",
            supplier.SupplierId,
            $"Supplier {supplier.SupplierId} updated. Name: {supplier.Name}.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SupplierDto>.Success(MapToDto(supplier));
    }

    public async Task<Result> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var supplierRepository = _unitOfWork.Repository<Supplier>();
        var supplier = await supplierRepository.GetByIdAsync(id, cancellationToken);

        if (supplier is null)
        {
            return Result.Failure(SupplierErrors.NotFound);
        }

        var partPurchaseRepository = _unitOfWork.Repository<PartPurchase>();
        var inUse = await partPurchaseRepository.ExistsAsync(
            x => x.SupplierId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(SupplierErrors.InUse);
        }

        var supplierName = supplier.Name;

        supplierRepository.Remove(supplier);
        await _auditLogger.LogAsync(
            currentUserId,
            DeleteAuditActionTypeName,
            "Suppliers",
            supplier.SupplierId,
            $"Supplier {supplier.SupplierId} deleted. Name: {supplierName}.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            TaxId = supplier.TaxId,
            Phone = supplier.Phone,
            Email = supplier.Email,
            IsActive = supplier.IsActive
        };
    }

    private static Error? Validate(string name, string? taxId, string? phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return SupplierErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return SupplierErrors.NameTooLong;
        }

        if (taxId is not null && taxId.Length > TaxIdMaxLength)
        {
            return SupplierErrors.TaxIdTooLong;
        }

        if (phone is not null && phone.Length > PhoneMaxLength)
        {
            return SupplierErrors.PhoneTooLong;
        }

        if (email is not null && email.Length > EmailMaxLength)
        {
            return SupplierErrors.EmailTooLong;
        }

        if (email is not null && !IsValidEmail(email))
        {
            return SupplierErrors.EmailInvalid;
        }

        return null;
    }

    private static string NormalizeRequiredText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeEmail(string? value)
    {
        var normalized = NormalizeOptionalText(value);
        return normalized?.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex != email.LastIndexOf('@'))
        {
            return false;
        }

        var domainPart = email[(atIndex + 1)..];
        if (string.IsNullOrWhiteSpace(domainPart))
        {
            return false;
        }

        return domainPart.Contains('.', StringComparison.Ordinal);
    }
}
