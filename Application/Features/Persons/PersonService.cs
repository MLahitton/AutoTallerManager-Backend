using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Persons.Dtos;
using Application.Features.Persons.Errors;
using Application.Features.Persons.Requests;
using Domain.Entities;

namespace Application.Features.Persons;

public class PersonService : IPersonService
{
    private const int DocumentNumberMaxLength = 30;
    private const int FirstNameMaxLength = 50;
    private const int MiddleNameMaxLength = 50;
    private const int LastNameMaxLength = 50;
    private const int SecondLastNameMaxLength = 50;

    private readonly IUnitOfWork _unitOfWork;

    public PersonService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PersonDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personRepository = _unitOfWork.Repository<Person>();
        var persons = await personRepository.GetAllAsync(cancellationToken);

        var personDtos = persons
            .OrderBy(x => x.PersonId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PersonDto>>.Success(personDtos);
    }

    public async Task<Result<PersonDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var personRepository = _unitOfWork.Repository<Person>();
        var person = await personRepository.GetByIdAsync(id, cancellationToken);

        if (person is null)
        {
            return Result<PersonDto>.Failure(PersonErrors.NotFound);
        }

        return Result<PersonDto>.Success(MapToDto(person));
    }

    public async Task<Result<PersonDto>> CreateAsync(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        var documentTypeId = request?.DocumentTypeId ?? 0;
        var documentNumber = NormalizeRequired(request?.DocumentNumber);
        var firstName = NormalizeRequired(request?.FirstName);
        var middleName = NormalizeOptional(request?.MiddleName);
        var lastName = NormalizeRequired(request?.LastName);
        var secondLastName = NormalizeOptional(request?.SecondLastName);
        var birthDate = request?.BirthDate;
        var genderId = request?.GenderId;
        var addressId = request?.AddressId;

        var validationError = Validate(
            documentTypeId,
            documentNumber,
            firstName,
            middleName,
            lastName,
            secondLastName,
            birthDate,
            genderId,
            addressId);

        if (validationError is not null)
        {
            return Result<PersonDto>.Failure(validationError);
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypeExists = await documentTypeRepository.ExistsAsync(
            x => x.DocumentTypeId == documentTypeId,
            cancellationToken);

        if (!documentTypeExists)
        {
            return Result<PersonDto>.Failure(PersonErrors.DocumentTypeNotFound);
        }

        if (genderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == genderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<PersonDto>.Failure(PersonErrors.GenderNotFound);
            }
        }

        if (addressId.HasValue)
        {
            var addressRepository = _unitOfWork.Repository<Address>();
            var addressExists = await addressRepository.ExistsAsync(
                x => x.AddressId == addressId.Value,
                cancellationToken);

            if (!addressExists)
            {
                return Result<PersonDto>.Failure(PersonErrors.AddressNotFound);
            }
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var documentNumberExists = await personRepository.ExistsAsync(
            x => x.DocumentNumber == documentNumber,
            cancellationToken);

        if (documentNumberExists)
        {
            return Result<PersonDto>.Failure(PersonErrors.DocumentNumberAlreadyExists);
        }

        var person = new Person
        {
            DocumentTypeId = documentTypeId,
            DocumentNumber = documentNumber,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            SecondLastName = secondLastName,
            BirthDate = birthDate,
            GenderId = genderId,
            AddressId = addressId
        };

        await personRepository.AddAsync(person, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonDto>.Success(MapToDto(person));
    }

    public async Task<Result<PersonDto>> UpdateAsync(int id, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        var personRepository = _unitOfWork.Repository<Person>();
        var person = await personRepository.GetByIdAsync(id, cancellationToken);

        if (person is null)
        {
            return Result<PersonDto>.Failure(PersonErrors.NotFound);
        }

        var documentTypeId = request?.DocumentTypeId ?? 0;
        var documentNumber = NormalizeRequired(request?.DocumentNumber);
        var firstName = NormalizeRequired(request?.FirstName);
        var middleName = NormalizeOptional(request?.MiddleName);
        var lastName = NormalizeRequired(request?.LastName);
        var secondLastName = NormalizeOptional(request?.SecondLastName);
        var birthDate = request?.BirthDate;
        var genderId = request?.GenderId;
        var addressId = request?.AddressId;

        var validationError = Validate(
            documentTypeId,
            documentNumber,
            firstName,
            middleName,
            lastName,
            secondLastName,
            birthDate,
            genderId,
            addressId);

        if (validationError is not null)
        {
            return Result<PersonDto>.Failure(validationError);
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypeExists = await documentTypeRepository.ExistsAsync(
            x => x.DocumentTypeId == documentTypeId,
            cancellationToken);

        if (!documentTypeExists)
        {
            return Result<PersonDto>.Failure(PersonErrors.DocumentTypeNotFound);
        }

        if (genderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == genderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<PersonDto>.Failure(PersonErrors.GenderNotFound);
            }
        }

        if (addressId.HasValue)
        {
            var addressRepository = _unitOfWork.Repository<Address>();
            var addressExists = await addressRepository.ExistsAsync(
                x => x.AddressId == addressId.Value,
                cancellationToken);

            if (!addressExists)
            {
                return Result<PersonDto>.Failure(PersonErrors.AddressNotFound);
            }
        }

        var documentNumberExists = await personRepository.ExistsAsync(
            x => x.DocumentNumber == documentNumber && x.PersonId != id,
            cancellationToken);

        if (documentNumberExists)
        {
            return Result<PersonDto>.Failure(PersonErrors.DocumentNumberAlreadyExists);
        }

        person.DocumentTypeId = documentTypeId;
        person.DocumentNumber = documentNumber;
        person.FirstName = firstName;
        person.MiddleName = middleName;
        person.LastName = lastName;
        person.SecondLastName = secondLastName;
        person.BirthDate = birthDate;
        person.GenderId = genderId;
        person.AddressId = addressId;

        personRepository.Update(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonDto>.Success(MapToDto(person));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var personRepository = _unitOfWork.Repository<Person>();
        var person = await personRepository.GetByIdAsync(id, cancellationToken);

        if (person is null)
        {
            return Result.Failure(PersonErrors.NotFound);
        }

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var inUseByEmail = await personEmailRepository.ExistsAsync(
            x => x.PersonId == id,
            cancellationToken);

        if (inUseByEmail)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var inUseByPhone = await personPhoneRepository.ExistsAsync(
            x => x.PersonId == id,
            cancellationToken);

        if (inUseByPhone)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var inUseByRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == id,
            cancellationToken);

        if (inUseByRole)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var inUseByUser = await userRepository.ExistsAsync(
            x => x.PersonId == id,
            cancellationToken);

        if (inUseByUser)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        var inUseByVehicleOwnership = await vehicleOwnerHistoryRepository.ExistsAsync(
            x => x.PersonId == id,
            cancellationToken);

        if (inUseByVehicleOwnership)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var inUseByMechanicAssignment = await mechanicAssignmentRepository.ExistsAsync(
            x => x.MechanicPersonId == id,
            cancellationToken);

        if (inUseByMechanicAssignment)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        var mechanicSpecialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var inUseByMechanicSpecialtyAssignment = await mechanicSpecialtyAssignmentRepository.ExistsAsync(
            x => x.PersonId == id,
            cancellationToken);

        if (inUseByMechanicSpecialtyAssignment)
        {
            return Result.Failure(PersonErrors.InUse);
        }

        personRepository.Remove(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PersonDto MapToDto(Person person)
    {
        return new PersonDto
        {
            PersonId = person.PersonId,
            DocumentTypeId = person.DocumentTypeId,
            DocumentNumber = person.DocumentNumber,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            SecondLastName = person.SecondLastName,
            BirthDate = person.BirthDate,
            GenderId = person.GenderId,
            AddressId = person.AddressId,
            CreatedAt = person.CreatedAt
        };
    }

    private static string NormalizeRequired(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static Error? Validate(
        int documentTypeId,
        string documentNumber,
        string firstName,
        string? middleName,
        string lastName,
        string? secondLastName,
        DateTime? birthDate,
        int? genderId,
        int? addressId)
    {
        if (documentTypeId <= 0)
        {
            return PersonErrors.DocumentTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return PersonErrors.DocumentNumberRequired;
        }

        if (documentNumber.Length > DocumentNumberMaxLength)
        {
            return PersonErrors.DocumentNumberTooLong;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return PersonErrors.FirstNameRequired;
        }

        if (firstName.Length > FirstNameMaxLength)
        {
            return PersonErrors.FirstNameTooLong;
        }

        if (middleName is not null && middleName.Length > MiddleNameMaxLength)
        {
            return PersonErrors.MiddleNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return PersonErrors.LastNameRequired;
        }

        if (lastName.Length > LastNameMaxLength)
        {
            return PersonErrors.LastNameTooLong;
        }

        if (secondLastName is not null && secondLastName.Length > SecondLastNameMaxLength)
        {
            return PersonErrors.SecondLastNameTooLong;
        }

        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
        {
            return PersonErrors.BirthDateInvalid;
        }

        if (genderId.HasValue && genderId.Value <= 0)
        {
            return PersonErrors.GenderIdInvalid;
        }

        if (addressId.HasValue && addressId.Value <= 0)
        {
            return PersonErrors.AddressIdInvalid;
        }

        return null;
    }
}
