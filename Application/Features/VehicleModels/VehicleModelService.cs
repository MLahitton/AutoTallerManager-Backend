using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.VehicleModels.Dtos;
using Application.Features.VehicleModels.Errors;
using Application.Features.VehicleModels.Requests;
using Domain.Entities;

namespace Application.Features.VehicleModels;

public class VehicleModelService : IVehicleModelService
{
    private const int ModelNameMaxLength = 80;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleModelDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var vehicleModels = await vehicleModelRepository.GetAllAsync(cancellationToken);

        var vehicleModelDtos = vehicleModels
            .OrderBy(x => x.ModelId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<VehicleModelDto>>.Success(vehicleModelDtos);
    }

    public async Task<Result<VehicleModelDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var vehicleModel = await vehicleModelRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleModel is null)
        {
            return Result<VehicleModelDto>.Failure(VehicleModelErrors.NotFound);
        }

        return Result<VehicleModelDto>.Success(MapToDto(vehicleModel));
    }

    public async Task<Result<VehicleModelDto>> CreateAsync(CreateVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        var brandId = request?.BrandId ?? 0;
        var modelName = NormalizeModelName(request?.ModelName);

        var validationError = Validate(brandId, modelName);
        if (validationError is not null)
        {
            return Result<VehicleModelDto>.Failure(validationError);
        }

        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var brandExists = await vehicleBrandRepository.ExistsAsync(
            x => x.BrandId == brandId,
            cancellationToken);

        if (!brandExists)
        {
            return Result<VehicleModelDto>.Failure(VehicleModelErrors.BrandNotFound);
        }

        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var modelNameAlreadyExists = await vehicleModelRepository.ExistsAsync(
            x => x.BrandId == brandId && x.ModelName == modelName,
            cancellationToken);

        if (modelNameAlreadyExists)
        {
            return Result<VehicleModelDto>.Failure(VehicleModelErrors.ModelNameAlreadyExists);
        }

        var vehicleModel = new VehicleModel
        {
            BrandId = brandId,
            ModelName = modelName
        };

        await vehicleModelRepository.AddAsync(vehicleModel, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleModelDto>.Success(MapToDto(vehicleModel));
    }

    public async Task<Result<VehicleModelDto>> UpdateAsync(int id, UpdateVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var vehicleModel = await vehicleModelRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleModel is null)
        {
            return Result<VehicleModelDto>.Failure(VehicleModelErrors.NotFound);
        }

        var brandId = request?.BrandId ?? 0;
        var modelName = NormalizeModelName(request?.ModelName);

        var validationError = Validate(brandId, modelName);
        if (validationError is not null)
        {
            return Result<VehicleModelDto>.Failure(validationError);
        }

        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var brandExists = await vehicleBrandRepository.ExistsAsync(
            x => x.BrandId == brandId,
            cancellationToken);

        if (!brandExists)
        {
            return Result<VehicleModelDto>.Failure(VehicleModelErrors.BrandNotFound);
        }

        var modelNameAlreadyExists = await vehicleModelRepository.ExistsAsync(
            x => x.BrandId == brandId && x.ModelName == modelName && x.ModelId != id,
            cancellationToken);

        if (modelNameAlreadyExists)
        {
            return Result<VehicleModelDto>.Failure(VehicleModelErrors.ModelNameAlreadyExists);
        }

        vehicleModel.BrandId = brandId;
        vehicleModel.ModelName = modelName;

        vehicleModelRepository.Update(vehicleModel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleModelDto>.Success(MapToDto(vehicleModel));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var vehicleModel = await vehicleModelRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleModel is null)
        {
            return Result.Failure(VehicleModelErrors.NotFound);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var inUse = await vehicleRepository.ExistsAsync(
            x => x.ModelId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(VehicleModelErrors.InUse);
        }

        vehicleModelRepository.Remove(vehicleModel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleModelDto MapToDto(VehicleModel vehicleModel)
    {
        return new VehicleModelDto
        {
            ModelId = vehicleModel.ModelId,
            BrandId = vehicleModel.BrandId,
            ModelName = vehicleModel.ModelName
        };
    }

    private static string NormalizeModelName(string? modelName)
    {
        return (modelName ?? string.Empty).Trim();
    }

    private static Error? Validate(int brandId, string modelName)
    {
        if (brandId <= 0)
        {
            return VehicleModelErrors.BrandIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            return VehicleModelErrors.ModelNameRequired;
        }

        if (modelName.Length > ModelNameMaxLength)
        {
            return VehicleModelErrors.ModelNameTooLong;
        }

        return null;
    }
}
