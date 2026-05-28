using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.VehicleTypes.Dtos;
using Application.Features.VehicleTypes.Errors;
using Application.Features.VehicleTypes.Requests;
using Domain.Entities;

namespace Application.Features.VehicleTypes;

public class VehicleTypeService : IVehicleTypeService
{
    private const int NameMaxLength = 80;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleTypes = await vehicleTypeRepository.GetAllAsync(cancellationToken);

        var vehicleTypeDtos = vehicleTypes
            .OrderBy(x => x.VehicleTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<VehicleTypeDto>>.Success(vehicleTypeDtos);
    }

    public async Task<Result<VehicleTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleType is null)
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NotFound);
        }

        return Result<VehicleTypeDto>.Success(MapToDto(vehicleType));
    }

    public async Task<Result<VehicleTypeDto>> CreateAsync(
        CreateVehicleTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<VehicleTypeDto>.Failure(validationError);
        }

        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var nameAlreadyExists = await vehicleTypeRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NameAlreadyExists);
        }

        var vehicleType = new VehicleType
        {
            Name = normalizedName
        };

        await vehicleTypeRepository.AddAsync(vehicleType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleTypeDto>.Success(MapToDto(vehicleType));
    }

    public async Task<Result<VehicleTypeDto>> UpdateAsync(
        int id,
        UpdateVehicleTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleType is null)
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<VehicleTypeDto>.Failure(validationError);
        }

        var nameAlreadyExists = await vehicleTypeRepository.ExistsAsync(
            x => x.Name == normalizedName && x.VehicleTypeId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NameAlreadyExists);
        }

        vehicleType.Name = normalizedName;

        vehicleTypeRepository.Update(vehicleType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleTypeDto>.Success(MapToDto(vehicleType));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleType = await vehicleTypeRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleType is null)
        {
            return Result.Failure(VehicleTypeErrors.NotFound);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var inUse = await vehicleRepository.ExistsAsync(
            x => x.VehicleTypeId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(VehicleTypeErrors.InUse);
        }

        vehicleTypeRepository.Remove(vehicleType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleTypeDto MapToDto(VehicleType vehicleType)
    {
        return new VehicleTypeDto
        {
            VehicleTypeId = vehicleType.VehicleTypeId,
            Name = vehicleType.Name
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
            return VehicleTypeErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return VehicleTypeErrors.NameTooLong;
        }

        return null;
    }
}
