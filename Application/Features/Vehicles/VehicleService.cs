using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Vehicles.Dtos;
using Application.Features.Vehicles.Errors;
using Application.Features.Vehicles.Requests;
using Domain.Entities;

namespace Application.Features.Vehicles;

public class VehicleService : IVehicleService
{
    private const int VinLength = 17;
    private const int ColorMaxLength = 30;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicles = await vehicleRepository.GetAllAsync(cancellationToken);

        var vehicleDtos = vehicles
            .OrderBy(x => x.VehicleId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<VehicleDto>>.Success(vehicleDtos);
    }

    public async Task<Result<VehicleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.NotFound);
        }

        return Result<VehicleDto>.Success(MapToDto(vehicle));
    }

    public async Task<Result<VehicleDto>> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var modelId = request?.ModelId ?? 0;
        var vehicleTypeId = request?.VehicleTypeId ?? 0;
        var vin = NormalizeVin(request?.VIN);
        var year = request?.Year ?? 0;
        var color = NormalizeColor(request?.Color);
        var mileage = request?.Mileage ?? 0;
        var isActive = request?.IsActive ?? false;

        var validationError = Validate(modelId, vehicleTypeId, vin, year, color, mileage);
        if (validationError is not null)
        {
            return Result<VehicleDto>.Failure(validationError);
        }

        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var modelExists = await vehicleModelRepository.ExistsAsync(
            x => x.ModelId == modelId,
            cancellationToken);

        if (!modelExists)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.ModelNotFound);
        }

        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleTypeExists = await vehicleTypeRepository.ExistsAsync(
            x => x.VehicleTypeId == vehicleTypeId,
            cancellationToken);

        if (!vehicleTypeExists)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.VehicleTypeNotFound);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vinAlreadyExists = await vehicleRepository.ExistsAsync(
            x => x.VIN == vin,
            cancellationToken);

        if (vinAlreadyExists)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.VinAlreadyExists);
        }

        var vehicle = new Vehicle
        {
            ModelId = modelId,
            VehicleTypeId = vehicleTypeId,
            VIN = vin,
            Year = year,
            Color = color,
            Mileage = mileage,
            IsActive = isActive
        };

        await vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleDto>.Success(MapToDto(vehicle));
    }

    public async Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.NotFound);
        }

        var modelId = request?.ModelId ?? 0;
        var vehicleTypeId = request?.VehicleTypeId ?? 0;
        var vin = NormalizeVin(request?.VIN);
        var year = request?.Year ?? 0;
        var color = NormalizeColor(request?.Color);
        var mileage = request?.Mileage ?? 0;
        var isActive = request?.IsActive ?? false;

        var validationError = Validate(modelId, vehicleTypeId, vin, year, color, mileage);
        if (validationError is not null)
        {
            return Result<VehicleDto>.Failure(validationError);
        }

        var vehicleModelRepository = _unitOfWork.Repository<VehicleModel>();
        var modelExists = await vehicleModelRepository.ExistsAsync(
            x => x.ModelId == modelId,
            cancellationToken);

        if (!modelExists)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.ModelNotFound);
        }

        var vehicleTypeRepository = _unitOfWork.Repository<VehicleType>();
        var vehicleTypeExists = await vehicleTypeRepository.ExistsAsync(
            x => x.VehicleTypeId == vehicleTypeId,
            cancellationToken);

        if (!vehicleTypeExists)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.VehicleTypeNotFound);
        }

        var vinAlreadyExists = await vehicleRepository.ExistsAsync(
            x => x.VIN == vin && x.VehicleId != id,
            cancellationToken);

        if (vinAlreadyExists)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.VinAlreadyExists);
        }

        vehicle.ModelId = modelId;
        vehicle.VehicleTypeId = vehicleTypeId;
        vehicle.VIN = vin;
        vehicle.Year = year;
        vehicle.Color = color;
        vehicle.Mileage = mileage;
        vehicle.IsActive = isActive;

        vehicleRepository.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleDto>.Success(MapToDto(vehicle));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var inUseByOwnerHistory = await vehicleOwnerHistoryRepository.ExistsAsync(
            x => x.VehicleId == id,
            cancellationToken);

        if (inUseByOwnerHistory)
        {
            return Result.Failure(VehicleErrors.InUse);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var inUseByServiceOrder = await serviceOrderRepository.ExistsAsync(
            x => x.VehicleId == id,
            cancellationToken);

        if (inUseByServiceOrder)
        {
            return Result.Failure(VehicleErrors.InUse);
        }

        vehicleRepository.Remove(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            VehicleId = vehicle.VehicleId,
            ModelId = vehicle.ModelId,
            VehicleTypeId = vehicle.VehicleTypeId,
            VIN = vehicle.VIN,
            Year = vehicle.Year,
            Color = vehicle.Color,
            Mileage = vehicle.Mileage,
            IsActive = vehicle.IsActive
        };
    }

    private static string NormalizeVin(string? vin)
    {
        return (vin ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string? NormalizeColor(string? color)
    {
        var normalized = (color ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static Error? Validate(int modelId, int vehicleTypeId, string vin, int year, string? color, int mileage)
    {
        if (modelId <= 0)
        {
            return VehicleErrors.ModelIdInvalid;
        }

        if (vehicleTypeId <= 0)
        {
            return VehicleErrors.VehicleTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(vin))
        {
            return VehicleErrors.VinRequired;
        }

        if (vin.Length > VinLength)
        {
            return VehicleErrors.VinTooLong;
        }

        if (vin.Length != VinLength || !vin.All(char.IsLetterOrDigit))
        {
            return VehicleErrors.VinInvalid;
        }

        var maxYear = DateTime.UtcNow.Year + 1;
        if (year < 1900 || year > maxYear)
        {
            return VehicleErrors.YearInvalid;
        }

        if (color is not null && color.Length > ColorMaxLength)
        {
            return VehicleErrors.ColorTooLong;
        }

        if (mileage < 0)
        {
            return VehicleErrors.MileageInvalid;
        }

        return null;
    }
}
