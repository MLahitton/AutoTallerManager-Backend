using Application.Common.Results;
using Application.Features.Cities.Dtos;
using Application.Features.Cities.Requests;

namespace Application.Features.Cities;

public interface ICityService
{
    Task<Result<IReadOnlyList<CityDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<CityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CityDto>> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default);

    Task<Result<CityDto>> UpdateAsync(int id, UpdateCityRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
