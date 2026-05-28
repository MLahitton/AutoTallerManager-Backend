using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Cities.Dtos;
using Application.Features.Cities.Errors;
using Application.Features.Cities.Requests;
using Domain.Entities;

namespace Application.Features.Cities;

public class CityService : ICityService
{
    private const int NameMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public CityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CityDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cityRepository = _unitOfWork.Repository<City>();
        var cities = await cityRepository.GetAllAsync(cancellationToken);

        var cityDtos = cities
            .OrderBy(x => x.CityId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<CityDto>>.Success(cityDtos);
    }

    public async Task<Result<CityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cityRepository = _unitOfWork.Repository<City>();
        var city = await cityRepository.GetByIdAsync(id, cancellationToken);

        if (city is null)
        {
            return Result<CityDto>.Failure(CityErrors.NotFound);
        }

        return Result<CityDto>.Success(MapToDto(city));
    }

    public async Task<Result<CityDto>> CreateAsync(
        CreateCityRequest request,
        CancellationToken cancellationToken = default)
    {
        var departmentId = request?.DepartmentId ?? 0;
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(departmentId, normalizedName);
        if (validationError is not null)
        {
            return Result<CityDto>.Failure(validationError);
        }

        var departmentRepository = _unitOfWork.Repository<Department>();
        var departmentExists = await departmentRepository.ExistsAsync(
            x => x.DepartmentId == departmentId,
            cancellationToken);

        if (!departmentExists)
        {
            return Result<CityDto>.Failure(CityErrors.DepartmentNotFound);
        }

        var cityRepository = _unitOfWork.Repository<City>();
        var nameAlreadyExists = await cityRepository.ExistsAsync(
            x => x.DepartmentId == departmentId && x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<CityDto>.Failure(CityErrors.NameAlreadyExists);
        }

        var city = new City
        {
            DepartmentId = departmentId,
            Name = normalizedName
        };

        await cityRepository.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CityDto>.Success(MapToDto(city));
    }

    public async Task<Result<CityDto>> UpdateAsync(
        int id,
        UpdateCityRequest request,
        CancellationToken cancellationToken = default)
    {
        var cityRepository = _unitOfWork.Repository<City>();
        var city = await cityRepository.GetByIdAsync(id, cancellationToken);

        if (city is null)
        {
            return Result<CityDto>.Failure(CityErrors.NotFound);
        }

        var departmentId = request?.DepartmentId ?? 0;
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(departmentId, normalizedName);
        if (validationError is not null)
        {
            return Result<CityDto>.Failure(validationError);
        }

        var departmentRepository = _unitOfWork.Repository<Department>();
        var departmentExists = await departmentRepository.ExistsAsync(
            x => x.DepartmentId == departmentId,
            cancellationToken);

        if (!departmentExists)
        {
            return Result<CityDto>.Failure(CityErrors.DepartmentNotFound);
        }

        var nameAlreadyExists = await cityRepository.ExistsAsync(
            x => x.DepartmentId == departmentId && x.Name == normalizedName && x.CityId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<CityDto>.Failure(CityErrors.NameAlreadyExists);
        }

        city.DepartmentId = departmentId;
        city.Name = normalizedName;

        cityRepository.Update(city);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CityDto>.Success(MapToDto(city));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cityRepository = _unitOfWork.Repository<City>();
        var city = await cityRepository.GetByIdAsync(id, cancellationToken);

        if (city is null)
        {
            return Result.Failure(CityErrors.NotFound);
        }

        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var inUse = await neighborhoodRepository.ExistsAsync(
            x => x.CityId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(CityErrors.InUse);
        }

        cityRepository.Remove(city);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static CityDto MapToDto(City city)
    {
        return new CityDto
        {
            CityId = city.CityId,
            DepartmentId = city.DepartmentId,
            Name = city.Name
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? Validate(int departmentId, string name)
    {
        if (departmentId <= 0)
        {
            return CityErrors.DepartmentIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return CityErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return CityErrors.NameTooLong;
        }

        return null;
    }
}
