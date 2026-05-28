using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Neighborhoods.Dtos;
using Application.Features.Neighborhoods.Errors;
using Application.Features.Neighborhoods.Requests;
using Domain.Entities;

namespace Application.Features.Neighborhoods;

public class NeighborhoodService : INeighborhoodService
{
    private const int NameMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public NeighborhoodService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<NeighborhoodDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var neighborhoods = await neighborhoodRepository.GetAllAsync(cancellationToken);

        var neighborhoodDtos = neighborhoods
            .OrderBy(x => x.NeighborhoodId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<NeighborhoodDto>>.Success(neighborhoodDtos);
    }

    public async Task<Result<NeighborhoodDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var neighborhood = await neighborhoodRepository.GetByIdAsync(id, cancellationToken);

        if (neighborhood is null)
        {
            return Result<NeighborhoodDto>.Failure(NeighborhoodErrors.NotFound);
        }

        return Result<NeighborhoodDto>.Success(MapToDto(neighborhood));
    }

    public async Task<Result<NeighborhoodDto>> CreateAsync(
        CreateNeighborhoodRequest request,
        CancellationToken cancellationToken = default)
    {
        var cityId = request?.CityId ?? 0;
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(cityId, normalizedName);
        if (validationError is not null)
        {
            return Result<NeighborhoodDto>.Failure(validationError);
        }

        var cityRepository = _unitOfWork.Repository<City>();
        var cityExists = await cityRepository.ExistsAsync(
            x => x.CityId == cityId,
            cancellationToken);

        if (!cityExists)
        {
            return Result<NeighborhoodDto>.Failure(NeighborhoodErrors.CityNotFound);
        }

        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var nameAlreadyExists = await neighborhoodRepository.ExistsAsync(
            x => x.CityId == cityId && x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<NeighborhoodDto>.Failure(NeighborhoodErrors.NameAlreadyExists);
        }

        var neighborhood = new Neighborhood
        {
            CityId = cityId,
            Name = normalizedName
        };

        await neighborhoodRepository.AddAsync(neighborhood, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NeighborhoodDto>.Success(MapToDto(neighborhood));
    }

    public async Task<Result<NeighborhoodDto>> UpdateAsync(
        int id,
        UpdateNeighborhoodRequest request,
        CancellationToken cancellationToken = default)
    {
        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var neighborhood = await neighborhoodRepository.GetByIdAsync(id, cancellationToken);

        if (neighborhood is null)
        {
            return Result<NeighborhoodDto>.Failure(NeighborhoodErrors.NotFound);
        }

        var cityId = request?.CityId ?? 0;
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(cityId, normalizedName);
        if (validationError is not null)
        {
            return Result<NeighborhoodDto>.Failure(validationError);
        }

        var cityRepository = _unitOfWork.Repository<City>();
        var cityExists = await cityRepository.ExistsAsync(
            x => x.CityId == cityId,
            cancellationToken);

        if (!cityExists)
        {
            return Result<NeighborhoodDto>.Failure(NeighborhoodErrors.CityNotFound);
        }

        var nameAlreadyExists = await neighborhoodRepository.ExistsAsync(
            x => x.CityId == cityId && x.Name == normalizedName && x.NeighborhoodId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<NeighborhoodDto>.Failure(NeighborhoodErrors.NameAlreadyExists);
        }

        neighborhood.CityId = cityId;
        neighborhood.Name = normalizedName;

        neighborhoodRepository.Update(neighborhood);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NeighborhoodDto>.Success(MapToDto(neighborhood));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var neighborhood = await neighborhoodRepository.GetByIdAsync(id, cancellationToken);

        if (neighborhood is null)
        {
            return Result.Failure(NeighborhoodErrors.NotFound);
        }

        var addressRepository = _unitOfWork.Repository<Address>();
        var inUse = await addressRepository.ExistsAsync(
            x => x.NeighborhoodId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(NeighborhoodErrors.InUse);
        }

        neighborhoodRepository.Remove(neighborhood);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static NeighborhoodDto MapToDto(Neighborhood neighborhood)
    {
        return new NeighborhoodDto
        {
            NeighborhoodId = neighborhood.NeighborhoodId,
            CityId = neighborhood.CityId,
            Name = neighborhood.Name
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? Validate(int cityId, string name)
    {
        if (cityId <= 0)
        {
            return NeighborhoodErrors.CityIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return NeighborhoodErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return NeighborhoodErrors.NameTooLong;
        }

        return null;
    }
}
