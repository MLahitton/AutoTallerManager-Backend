using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.StreetTypes.Dtos;
using Application.Features.StreetTypes.Errors;
using Application.Features.StreetTypes.Requests;
using Domain.Entities;

namespace Application.Features.StreetTypes;

public class StreetTypeService : IStreetTypeService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public StreetTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<StreetTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var streetTypes = await streetTypeRepository.GetAllAsync(cancellationToken);

        var streetTypeDtos = streetTypes
            .OrderBy(x => x.StreetTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<StreetTypeDto>>.Success(streetTypeDtos);
    }

    public async Task<Result<StreetTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var streetType = await streetTypeRepository.GetByIdAsync(id, cancellationToken);

        if (streetType is null)
        {
            return Result<StreetTypeDto>.Failure(StreetTypeErrors.NotFound);
        }

        return Result<StreetTypeDto>.Success(MapToDto(streetType));
    }

    public async Task<Result<StreetTypeDto>> CreateAsync(
        CreateStreetTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<StreetTypeDto>.Failure(validationError);
        }

        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var nameAlreadyExists = await streetTypeRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<StreetTypeDto>.Failure(StreetTypeErrors.NameAlreadyExists);
        }

        var streetType = new StreetType
        {
            Name = normalizedName
        };

        await streetTypeRepository.AddAsync(streetType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StreetTypeDto>.Success(MapToDto(streetType));
    }

    public async Task<Result<StreetTypeDto>> UpdateAsync(
        int id,
        UpdateStreetTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var streetType = await streetTypeRepository.GetByIdAsync(id, cancellationToken);

        if (streetType is null)
        {
            return Result<StreetTypeDto>.Failure(StreetTypeErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<StreetTypeDto>.Failure(validationError);
        }

        var nameAlreadyExists = await streetTypeRepository.ExistsAsync(
            x => x.Name == normalizedName && x.StreetTypeId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<StreetTypeDto>.Failure(StreetTypeErrors.NameAlreadyExists);
        }

        streetType.Name = normalizedName;

        streetTypeRepository.Update(streetType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StreetTypeDto>.Success(MapToDto(streetType));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var streetType = await streetTypeRepository.GetByIdAsync(id, cancellationToken);

        if (streetType is null)
        {
            return Result.Failure(StreetTypeErrors.NotFound);
        }

        var addressRepository = _unitOfWork.Repository<Address>();
        var inUse = await addressRepository.ExistsAsync(
            x => x.StreetTypeId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(StreetTypeErrors.InUse);
        }

        streetTypeRepository.Remove(streetType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static StreetTypeDto MapToDto(StreetType streetType)
    {
        return new StreetTypeDto
        {
            StreetTypeId = streetType.StreetTypeId,
            Name = streetType.Name
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
            return StreetTypeErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return StreetTypeErrors.NameTooLong;
        }

        return null;
    }
}
