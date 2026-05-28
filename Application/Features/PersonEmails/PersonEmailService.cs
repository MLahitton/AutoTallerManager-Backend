using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PersonEmails.Dtos;
using Application.Features.PersonEmails.Errors;
using Application.Features.PersonEmails.Requests;
using Domain.Entities;

namespace Application.Features.PersonEmails;

public class PersonEmailService : IPersonEmailService
{
    private const int EmailUserMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public PersonEmailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PersonEmailDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmails = await personEmailRepository.GetAllAsync(cancellationToken);

        var personEmailDtos = personEmails
            .OrderBy(x => x.PersonEmailId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PersonEmailDto>>.Success(personEmailDtos);
    }

    public async Task<Result<PersonEmailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmail = await personEmailRepository.GetByIdAsync(id, cancellationToken);

        if (personEmail is null)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.NotFound);
        }

        return Result<PersonEmailDto>.Success(MapToDto(personEmail));
    }

    public async Task<Result<PersonEmailDto>> CreateAsync(CreatePersonEmailRequest request, CancellationToken cancellationToken = default)
    {
        var personId = request?.PersonId ?? 0;
        var emailDomainId = request?.EmailDomainId ?? 0;
        var emailUser = NormalizeEmailUser(request?.EmailUser);
        var isPrimary = request?.IsPrimary ?? false;

        var validationError = Validate(personId, emailDomainId, emailUser);
        if (validationError is not null)
        {
            return Result<PersonEmailDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.PersonNotFound);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomainExists = await emailDomainRepository.ExistsAsync(
            x => x.EmailDomainId == emailDomainId,
            cancellationToken);

        if (!emailDomainExists)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.EmailDomainNotFound);
        }

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var emailAlreadyExists = await personEmailRepository.ExistsAsync(
            x => x.EmailUser == emailUser && x.EmailDomainId == emailDomainId,
            cancellationToken);

        if (emailAlreadyExists)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.EmailAlreadyExists);
        }

        if (isPrimary)
        {
            var primaryAlreadyExists = await personEmailRepository.ExistsAsync(
                x => x.PersonId == personId && x.IsPrimary,
                cancellationToken);

            if (primaryAlreadyExists)
            {
                return Result<PersonEmailDto>.Failure(PersonEmailErrors.PrimaryAlreadyExists);
            }
        }

        var personEmail = new PersonEmail
        {
            PersonId = personId,
            EmailDomainId = emailDomainId,
            EmailUser = emailUser,
            IsPrimary = isPrimary
        };

        await personEmailRepository.AddAsync(personEmail, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonEmailDto>.Success(MapToDto(personEmail));
    }

    public async Task<Result<PersonEmailDto>> UpdateAsync(int id, UpdatePersonEmailRequest request, CancellationToken cancellationToken = default)
    {
        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmail = await personEmailRepository.GetByIdAsync(id, cancellationToken);

        if (personEmail is null)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.NotFound);
        }

        var personId = request?.PersonId ?? 0;
        var emailDomainId = request?.EmailDomainId ?? 0;
        var emailUser = NormalizeEmailUser(request?.EmailUser);
        var isPrimary = request?.IsPrimary ?? false;

        var validationError = Validate(personId, emailDomainId, emailUser);
        if (validationError is not null)
        {
            return Result<PersonEmailDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.PersonNotFound);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomainExists = await emailDomainRepository.ExistsAsync(
            x => x.EmailDomainId == emailDomainId,
            cancellationToken);

        if (!emailDomainExists)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.EmailDomainNotFound);
        }

        var emailAlreadyExists = await personEmailRepository.ExistsAsync(
            x => x.EmailUser == emailUser && x.EmailDomainId == emailDomainId && x.PersonEmailId != id,
            cancellationToken);

        if (emailAlreadyExists)
        {
            return Result<PersonEmailDto>.Failure(PersonEmailErrors.EmailAlreadyExists);
        }

        if (isPrimary)
        {
            var primaryAlreadyExists = await personEmailRepository.ExistsAsync(
                x => x.PersonId == personId && x.IsPrimary && x.PersonEmailId != id,
                cancellationToken);

            if (primaryAlreadyExists)
            {
                return Result<PersonEmailDto>.Failure(PersonEmailErrors.PrimaryAlreadyExists);
            }
        }

        personEmail.PersonId = personId;
        personEmail.EmailDomainId = emailDomainId;
        personEmail.EmailUser = emailUser;
        personEmail.IsPrimary = isPrimary;

        personEmailRepository.Update(personEmail);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonEmailDto>.Success(MapToDto(personEmail));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmail = await personEmailRepository.GetByIdAsync(id, cancellationToken);

        if (personEmail is null)
        {
            return Result.Failure(PersonEmailErrors.NotFound);
        }

        personEmailRepository.Remove(personEmail);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PersonEmailDto MapToDto(PersonEmail personEmail)
    {
        return new PersonEmailDto
        {
            PersonEmailId = personEmail.PersonEmailId,
            PersonId = personEmail.PersonId,
            EmailDomainId = personEmail.EmailDomainId,
            EmailUser = personEmail.EmailUser,
            IsPrimary = personEmail.IsPrimary
        };
    }

    private static string NormalizeEmailUser(string? emailUser)
    {
        return (emailUser ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static Error? Validate(int personId, int emailDomainId, string emailUser)
    {
        if (personId <= 0)
        {
            return PersonEmailErrors.PersonIdInvalid;
        }

        if (emailDomainId <= 0)
        {
            return PersonEmailErrors.EmailDomainIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(emailUser))
        {
            return PersonEmailErrors.EmailUserRequired;
        }

        if (emailUser.Length > EmailUserMaxLength)
        {
            return PersonEmailErrors.EmailUserTooLong;
        }

        if (emailUser.Contains('@'))
        {
            return PersonEmailErrors.EmailUserInvalid;
        }

        return null;
    }
}
