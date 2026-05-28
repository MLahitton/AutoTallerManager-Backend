using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Countries.Dtos;
using Application.Features.Countries.Errors;
using Application.Features.Countries.Requests;
using Domain.Entities;

namespace Application.Features.Countries;

public class CountryService : ICountryService
{
    private const int NameMaxLength = 100;
    private const int PhoneCodeMaxLength = 10;
    private readonly IUnitOfWork _unitOfWork;

    public CountryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CountryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var countryRepository = _unitOfWork.Repository<Country>();
        var countries = await countryRepository.GetAllAsync(cancellationToken);

        var countryDtos = countries
            .OrderBy(x => x.CountryId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<CountryDto>>.Success(countryDtos);
    }

    public async Task<Result<CountryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var countryRepository = _unitOfWork.Repository<Country>();
        var country = await countryRepository.GetByIdAsync(id, cancellationToken);

        if (country is null)
        {
            return Result<CountryDto>.Failure(CountryErrors.NotFound);
        }

        return Result<CountryDto>.Success(MapToDto(country));
    }

    public async Task<Result<CountryDto>> CreateAsync(
        CreateCountryRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var normalizedPhoneCode = NormalizePhoneCode(request?.PhoneCode);

        var validationError = Validate(normalizedName, normalizedPhoneCode);
        if (validationError is not null)
        {
            return Result<CountryDto>.Failure(validationError);
        }

        var countryRepository = _unitOfWork.Repository<Country>();
        var nameAlreadyExists = await countryRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<CountryDto>.Failure(CountryErrors.NameAlreadyExists);
        }

        var country = new Country
        {
            Name = normalizedName,
            PhoneCode = normalizedPhoneCode
        };

        await countryRepository.AddAsync(country, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CountryDto>.Success(MapToDto(country));
    }

    public async Task<Result<CountryDto>> UpdateAsync(
        int id,
        UpdateCountryRequest request,
        CancellationToken cancellationToken = default)
    {
        var countryRepository = _unitOfWork.Repository<Country>();
        var country = await countryRepository.GetByIdAsync(id, cancellationToken);

        if (country is null)
        {
            return Result<CountryDto>.Failure(CountryErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var normalizedPhoneCode = NormalizePhoneCode(request?.PhoneCode);

        var validationError = Validate(normalizedName, normalizedPhoneCode);
        if (validationError is not null)
        {
            return Result<CountryDto>.Failure(validationError);
        }

        var nameAlreadyExists = await countryRepository.ExistsAsync(
            x => x.Name == normalizedName && x.CountryId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<CountryDto>.Failure(CountryErrors.NameAlreadyExists);
        }

        country.Name = normalizedName;
        country.PhoneCode = normalizedPhoneCode;

        countryRepository.Update(country);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CountryDto>.Success(MapToDto(country));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var countryRepository = _unitOfWork.Repository<Country>();
        var country = await countryRepository.GetByIdAsync(id, cancellationToken);

        if (country is null)
        {
            return Result.Failure(CountryErrors.NotFound);
        }

        var departmentRepository = _unitOfWork.Repository<Department>();
        var inUseByDepartments = await departmentRepository.ExistsAsync(
            x => x.CountryId == id,
            cancellationToken);

        if (inUseByDepartments)
        {
            return Result.Failure(CountryErrors.InUse);
        }

        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var inUseByPersonPhones = await personPhoneRepository.ExistsAsync(
            x => x.CountryId == id,
            cancellationToken);

        if (inUseByPersonPhones)
        {
            return Result.Failure(CountryErrors.InUse);
        }

        countryRepository.Remove(country);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static CountryDto MapToDto(Country country)
    {
        return new CountryDto
        {
            CountryId = country.CountryId,
            Name = country.Name,
            PhoneCode = country.PhoneCode
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static string? NormalizePhoneCode(string? phoneCode)
    {
        var normalized = (phoneCode ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static Error? Validate(string name, string? phoneCode)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CountryErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return CountryErrors.NameTooLong;
        }

        if (phoneCode is not null && phoneCode.Length > PhoneCodeMaxLength)
        {
            return CountryErrors.PhoneCodeTooLong;
        }

        return null;
    }
}
