using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PersonPhones.Dtos;
using Application.Features.PersonPhones.Errors;
using Application.Features.PersonPhones.Requests;
using Domain.Entities;

namespace Application.Features.PersonPhones;

public class PersonPhoneService : IPersonPhoneService
{
    private const int PhoneNumberMaxLength = 20;
    private readonly IUnitOfWork _unitOfWork;

    public PersonPhoneService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PersonPhoneDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var personPhones = await personPhoneRepository.GetAllAsync(cancellationToken);

        var personPhoneDtos = personPhones
            .OrderBy(x => x.PersonPhoneId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PersonPhoneDto>>.Success(personPhoneDtos);
    }

    public async Task<Result<PersonPhoneDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var personPhone = await personPhoneRepository.GetByIdAsync(id, cancellationToken);

        if (personPhone is null)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.NotFound);
        }

        return Result<PersonPhoneDto>.Success(MapToDto(personPhone));
    }

    public async Task<Result<PersonPhoneDto>> CreateAsync(CreatePersonPhoneRequest request, CancellationToken cancellationToken = default)
    {
        var personId = request?.PersonId ?? 0;
        var countryId = request?.CountryId ?? 0;
        var phoneNumber = NormalizePhoneNumber(request?.PhoneNumber);
        var isPrimary = request?.IsPrimary ?? false;

        var validationError = Validate(personId, countryId, phoneNumber);
        if (validationError is not null)
        {
            return Result<PersonPhoneDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.PersonNotFound);
        }

        var countryRepository = _unitOfWork.Repository<Country>();
        var countryExists = await countryRepository.ExistsAsync(
            x => x.CountryId == countryId,
            cancellationToken);

        if (!countryExists)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.CountryNotFound);
        }

        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var phoneAlreadyExists = await personPhoneRepository.ExistsAsync(
            x => x.CountryId == countryId && x.PhoneNumber == phoneNumber,
            cancellationToken);

        if (phoneAlreadyExists)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.PhoneNumberAlreadyExists);
        }

        if (isPrimary)
        {
            var primaryAlreadyExists = await personPhoneRepository.ExistsAsync(
                x => x.PersonId == personId && x.IsPrimary,
                cancellationToken);

            if (primaryAlreadyExists)
            {
                return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.PrimaryAlreadyExists);
            }
        }

        var personPhone = new PersonPhone
        {
            PersonId = personId,
            CountryId = countryId,
            PhoneNumber = phoneNumber,
            IsPrimary = isPrimary
        };

        await personPhoneRepository.AddAsync(personPhone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonPhoneDto>.Success(MapToDto(personPhone));
    }

    public async Task<Result<PersonPhoneDto>> UpdateAsync(int id, UpdatePersonPhoneRequest request, CancellationToken cancellationToken = default)
    {
        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var personPhone = await personPhoneRepository.GetByIdAsync(id, cancellationToken);

        if (personPhone is null)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.NotFound);
        }

        var personId = request?.PersonId ?? 0;
        var countryId = request?.CountryId ?? 0;
        var phoneNumber = NormalizePhoneNumber(request?.PhoneNumber);
        var isPrimary = request?.IsPrimary ?? false;

        var validationError = Validate(personId, countryId, phoneNumber);
        if (validationError is not null)
        {
            return Result<PersonPhoneDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.PersonNotFound);
        }

        var countryRepository = _unitOfWork.Repository<Country>();
        var countryExists = await countryRepository.ExistsAsync(
            x => x.CountryId == countryId,
            cancellationToken);

        if (!countryExists)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.CountryNotFound);
        }

        var phoneAlreadyExists = await personPhoneRepository.ExistsAsync(
            x => x.CountryId == countryId && x.PhoneNumber == phoneNumber && x.PersonPhoneId != id,
            cancellationToken);

        if (phoneAlreadyExists)
        {
            return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.PhoneNumberAlreadyExists);
        }

        if (isPrimary)
        {
            var primaryAlreadyExists = await personPhoneRepository.ExistsAsync(
                x => x.PersonId == personId && x.IsPrimary && x.PersonPhoneId != id,
                cancellationToken);

            if (primaryAlreadyExists)
            {
                return Result<PersonPhoneDto>.Failure(PersonPhoneErrors.PrimaryAlreadyExists);
            }
        }

        personPhone.PersonId = personId;
        personPhone.CountryId = countryId;
        personPhone.PhoneNumber = phoneNumber;
        personPhone.IsPrimary = isPrimary;

        personPhoneRepository.Update(personPhone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonPhoneDto>.Success(MapToDto(personPhone));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var personPhone = await personPhoneRepository.GetByIdAsync(id, cancellationToken);

        if (personPhone is null)
        {
            return Result.Failure(PersonPhoneErrors.NotFound);
        }

        personPhoneRepository.Remove(personPhone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PersonPhoneDto MapToDto(PersonPhone personPhone)
    {
        return new PersonPhoneDto
        {
            PersonPhoneId = personPhone.PersonPhoneId,
            PersonId = personPhone.PersonId,
            CountryId = personPhone.CountryId,
            PhoneNumber = personPhone.PhoneNumber,
            IsPrimary = personPhone.IsPrimary
        };
    }

    private static string NormalizePhoneNumber(string? phoneNumber)
    {
        return (phoneNumber ?? string.Empty).Trim();
    }

    private static Error? Validate(int personId, int countryId, string phoneNumber)
    {
        if (personId <= 0)
        {
            return PersonPhoneErrors.PersonIdInvalid;
        }

        if (countryId <= 0)
        {
            return PersonPhoneErrors.CountryIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return PersonPhoneErrors.PhoneNumberRequired;
        }

        if (phoneNumber.Length > PhoneNumberMaxLength)
        {
            return PersonPhoneErrors.PhoneNumberTooLong;
        }

        if (!IsValidPhoneNumber(phoneNumber))
        {
            return PersonPhoneErrors.PhoneNumberInvalid;
        }

        return null;
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
    var digitCount = 0;

    for (var i = 0; i < phoneNumber.Length; i++)
    {
        var c = phoneNumber[i];

        if (i == 0 && c == '+')
        {
            continue;
        }

        if (!char.IsDigit(c))
        {
            return false;
        }

        digitCount++;
    }

    return digitCount > 0;
    }
}
