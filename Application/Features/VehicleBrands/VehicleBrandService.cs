using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.VehicleBrands.Dtos;
using Application.Features.VehicleBrands.Errors;
using Application.Features.VehicleBrands.Requests;
using Domain.Entities;

namespace Application.Features.VehicleBrands;

public class VehicleBrandService : IVehicleBrandService
{
    private const int BrandNameMaxLength = 80;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleBrandService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleBrandDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var vehicleBrands = await vehicleBrandRepository.GetAllAsync(cancellationToken);

        var vehicleBrandDtos = vehicleBrands
            .OrderBy(x => x.BrandId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<VehicleBrandDto>>.Success(vehicleBrandDtos);
    }

    public async Task<Result<VehicleBrandDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var vehicleBrand = await vehicleBrandRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleBrand is null)
        {
            return Result<VehicleBrandDto>.Failure(VehicleBrandErrors.NotFound);
        }

        return Result<VehicleBrandDto>.Success(MapToDto(vehicleBrand));
    }

    public async Task<Result<VehicleBrandDto>> CreateAsync(
        CreateVehicleBrandRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedBrandName = NormalizeBrandName(request?.BrandName);
        var validationError = ValidateBrandName(normalizedBrandName);
        if (validationError is not null)
        {
            return Result<VehicleBrandDto>.Failure(validationError);
        }

        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var brandNameAlreadyExists = await vehicleBrandRepository.ExistsAsync(
            x => x.BrandName == normalizedBrandName,
            cancellationToken);

        if (brandNameAlreadyExists)
        {
            return Result<VehicleBrandDto>.Failure(VehicleBrandErrors.BrandNameAlreadyExists);
        }

        var vehicleBrand = new VehicleBrand
        {
            BrandName = normalizedBrandName
        };

        await vehicleBrandRepository.AddAsync(vehicleBrand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleBrandDto>.Success(MapToDto(vehicleBrand));
    }

    public async Task<Result<VehicleBrandDto>> UpdateAsync(
        int id,
        UpdateVehicleBrandRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var vehicleBrand = await vehicleBrandRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleBrand is null)
        {
            return Result<VehicleBrandDto>.Failure(VehicleBrandErrors.NotFound);
        }

        var normalizedBrandName = NormalizeBrandName(request?.BrandName);
        var validationError = ValidateBrandName(normalizedBrandName);
        if (validationError is not null)
        {
            return Result<VehicleBrandDto>.Failure(validationError);
        }

        var brandNameAlreadyExists = await vehicleBrandRepository.ExistsAsync(
            x => x.BrandName == normalizedBrandName && x.BrandId != id,
            cancellationToken);

        if (brandNameAlreadyExists)
        {
            return Result<VehicleBrandDto>.Failure(VehicleBrandErrors.BrandNameAlreadyExists);
        }

        vehicleBrand.BrandName = normalizedBrandName;

        vehicleBrandRepository.Update(vehicleBrand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleBrandDto>.Success(MapToDto(vehicleBrand));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleBrandRepository = _unitOfWork.Repository<VehicleBrand>();
        var vehicleBrand = await vehicleBrandRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleBrand is null)
        {
            return Result.Failure(VehicleBrandErrors.NotFound);
        }

        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var inUse = await vehicleModelRepository.ExistsAsync(
            x => x.BrandId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(VehicleBrandErrors.InUse);
        }

        vehicleBrandRepository.Remove(vehicleBrand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleBrandDto MapToDto(VehicleBrand vehicleBrand)
    {
        return new VehicleBrandDto
        {
            BrandId = vehicleBrand.BrandId,
            BrandName = vehicleBrand.BrandName
        };
    }

    private static string NormalizeBrandName(string? brandName)
    {
        return (brandName ?? string.Empty).Trim();
    }

    private static Error? ValidateBrandName(string brandName)
    {
        if (string.IsNullOrWhiteSpace(brandName))
        {
            return VehicleBrandErrors.BrandNameRequired;
        }

        if (brandName.Length > BrandNameMaxLength)
        {
            return VehicleBrandErrors.BrandNameTooLong;
        }

        return null;
    }
}
