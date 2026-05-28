using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.AuditActionTypes.Dtos;
using Application.Features.AuditActionTypes.Errors;
using Application.Features.AuditActionTypes.Requests;
using Domain.Entities;

namespace Application.Features.AuditActionTypes;

public class AuditActionTypeService : IAuditActionTypeService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public AuditActionTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AuditActionTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionTypes = await auditActionTypeRepository.GetAllAsync(cancellationToken);

        var auditActionTypeDtos = auditActionTypes
            .OrderBy(x => x.AuditActionTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<AuditActionTypeDto>>.Success(auditActionTypeDtos);
    }

    public async Task<Result<AuditActionTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionType = await auditActionTypeRepository.GetByIdAsync(id, cancellationToken);

        if (auditActionType is null)
        {
            return Result<AuditActionTypeDto>.Failure(AuditActionTypeErrors.NotFound);
        }

        return Result<AuditActionTypeDto>.Success(MapToDto(auditActionType));
    }

    public async Task<Result<AuditActionTypeDto>> CreateAsync(
        CreateAuditActionTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<AuditActionTypeDto>.Failure(validationError);
        }

        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var nameAlreadyExists = await auditActionTypeRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<AuditActionTypeDto>.Failure(AuditActionTypeErrors.NameAlreadyExists);
        }

        var auditActionType = new AuditActionType
        {
            Name = normalizedName
        };

        await auditActionTypeRepository.AddAsync(auditActionType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuditActionTypeDto>.Success(MapToDto(auditActionType));
    }

    public async Task<Result<AuditActionTypeDto>> UpdateAsync(
        int id,
        UpdateAuditActionTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionType = await auditActionTypeRepository.GetByIdAsync(id, cancellationToken);

        if (auditActionType is null)
        {
            return Result<AuditActionTypeDto>.Failure(AuditActionTypeErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<AuditActionTypeDto>.Failure(validationError);
        }

        var nameAlreadyExists = await auditActionTypeRepository.ExistsAsync(
            x => x.Name == normalizedName && x.AuditActionTypeId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<AuditActionTypeDto>.Failure(AuditActionTypeErrors.NameAlreadyExists);
        }

        auditActionType.Name = normalizedName;

        auditActionTypeRepository.Update(auditActionType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuditActionTypeDto>.Success(MapToDto(auditActionType));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var auditActionTypeRepository = _unitOfWork.Repository<AuditActionType>();
        var auditActionType = await auditActionTypeRepository.GetByIdAsync(id, cancellationToken);

        if (auditActionType is null)
        {
            return Result.Failure(AuditActionTypeErrors.NotFound);
        }

        var auditRepository = _unitOfWork.Repository<Audit>();
        var inUse = await auditRepository.ExistsAsync(
            x => x.AuditActionTypeId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(AuditActionTypeErrors.InUse);
        }

        auditActionTypeRepository.Remove(auditActionType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static AuditActionTypeDto MapToDto(AuditActionType auditActionType)
    {
        return new AuditActionTypeDto
        {
            AuditActionTypeId = auditActionType.AuditActionTypeId,
            Name = auditActionType.Name
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AuditActionTypeErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return AuditActionTypeErrors.NameTooLong;
        }

        return null;
    }
}
