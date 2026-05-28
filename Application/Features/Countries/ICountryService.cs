using Application.Common.Results;
using Application.Features.Countries.Dtos;
using Application.Features.Countries.Requests;

namespace Application.Features.Countries;

public interface ICountryService
{
    Task<Result<IReadOnlyList<CountryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<CountryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CountryDto>> CreateAsync(CreateCountryRequest request, CancellationToken cancellationToken = default);

    Task<Result<CountryDto>> UpdateAsync(int id, UpdateCountryRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
