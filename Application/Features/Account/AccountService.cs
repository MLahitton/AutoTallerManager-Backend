using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Common.Security;
using Application.Features.Account.Dtos;
using Application.Features.Account.Errors;
using Application.Features.Account.Requests;
using Domain.Entities;

namespace Application.Features.Account;

public class AccountService : IAccountService
{
    private const int FirstNameMaxLength = 50;
    private const int MiddleNameMaxLength = 50;
    private const int LastNameMaxLength = 50;
    private const int SecondLastNameMaxLength = 50;
    private const int EmailUserMaxLength = 100;
    private const int PhoneNumberMaxLength = 20;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public AccountService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AccountProfileDto>> GetMeAsync(int userId, int personId, CancellationToken cancellationToken = default)
    {
        var resolution = await ResolveUserAndPersonAsync(userId, personId, validateActiveUser: true, cancellationToken);
        if (resolution.Error is not null)
        {
            return Result<AccountProfileDto>.Failure(resolution.Error);
        }

        var profile = await BuildProfileDtoAsync(resolution.User!, resolution.Person!, cancellationToken);
        return Result<AccountProfileDto>.Success(profile);
    }

    public async Task<Result<AccountProfileDto>> UpdateMeAsync(
        int userId,
        int personId,
        UpdateAccountProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var resolution = await ResolveUserAndPersonAsync(userId, personId, validateActiveUser: true, cancellationToken);
        if (resolution.Error is not null)
        {
            return Result<AccountProfileDto>.Failure(resolution.Error);
        }

        var user = resolution.User!;
        var person = resolution.Person!;

        var firstName = NormalizeRequiredText(request?.FirstName);
        var middleName = NormalizeOptionalText(request?.MiddleName);
        var lastName = NormalizeRequiredText(request?.LastName);
        var secondLastName = NormalizeOptionalText(request?.SecondLastName);
        var birthDate = request?.BirthDate;
        var genderId = request?.GenderId;
        var addressId = request?.AddressId;

        var validationError = ValidateProfileFields(firstName, middleName, lastName, secondLastName, birthDate, genderId, addressId);
        if (validationError is not null)
        {
            return Result<AccountProfileDto>.Failure(validationError);
        }

        if (genderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == genderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.GenderNotFound);
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
                return Result<AccountProfileDto>.Failure(AccountErrors.AddressNotFound);
            }
        }

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();

        var emailProvided = request?.Email is not null;
        if (emailProvided)
        {
            var normalizedEmail = NormalizeEmail(request!.Email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.EmailInvalid);
            }

            if (!TrySplitEmail(normalizedEmail, out var emailUser, out var domain))
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.EmailInvalid);
            }

            if (emailUser.Length > EmailUserMaxLength)
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.EmailInvalid);
            }

            var personEmails = await personEmailRepository.FindAsync(
                x => x.PersonId == person.PersonId,
                cancellationToken);
            var primaryPersonEmail = personEmails
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.PersonEmailId)
                .FirstOrDefault();

            var existingEmailDomains = await emailDomainRepository.FindAsync(
                x => x.Domain == domain,
                cancellationToken);
            var emailDomain = existingEmailDomains.FirstOrDefault();

            if (emailDomain is not null)
            {
                var primaryPersonEmailIdToExclude = primaryPersonEmail?.PersonEmailId ?? 0;
                var emailAlreadyExists = await personEmailRepository.ExistsAsync(
                    x => x.EmailUser == emailUser &&
                         x.EmailDomainId == emailDomain.EmailDomainId &&
                         x.PersonEmailId != primaryPersonEmailIdToExclude,
                    cancellationToken);

                if (emailAlreadyExists)
                {
                    return Result<AccountProfileDto>.Failure(AccountErrors.EmailAlreadyExists);
                }
            }
            else
            {
                emailDomain = new EmailDomain
                {
                    Domain = domain
                };

                await emailDomainRepository.AddAsync(emailDomain, cancellationToken);
            }

            if (primaryPersonEmail is not null)
            {
                primaryPersonEmail.EmailUser = emailUser;
                primaryPersonEmail.IsPrimary = true;

                if (emailDomain.EmailDomainId > 0)
                {
                    primaryPersonEmail.EmailDomainId = emailDomain.EmailDomainId;
                }
                else
                {
                    primaryPersonEmail.EmailDomain = emailDomain;
                }

                personEmailRepository.Update(primaryPersonEmail);
            }
            else
            {
                var newPrimaryEmail = new PersonEmail
                {
                    PersonId = person.PersonId,
                    EmailUser = emailUser,
                    IsPrimary = true
                };

                if (emailDomain.EmailDomainId > 0)
                {
                    newPrimaryEmail.EmailDomainId = emailDomain.EmailDomainId;
                }
                else
                {
                    newPrimaryEmail.EmailDomain = emailDomain;
                }

                await personEmailRepository.AddAsync(newPrimaryEmail, cancellationToken);
            }
        }

        var phoneProvided = request?.PhoneNumber is not null;
        if (phoneProvided)
        {
            var phoneNumber = NormalizeRequiredText(request!.PhoneNumber);
            if (string.IsNullOrWhiteSpace(phoneNumber) || !IsValidPhoneNumber(phoneNumber))
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.PhoneNumberInvalid);
            }

            if (phoneNumber.Length > PhoneNumberMaxLength)
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.PhoneNumberTooLong);
            }

            if (!request.PhoneCountryId.HasValue || request.PhoneCountryId.Value <= 0)
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.PhoneCountryIdRequired);
            }

            var countryRepository = _unitOfWork.Repository<Country>();
            var countryExists = await countryRepository.ExistsAsync(
                x => x.CountryId == request.PhoneCountryId.Value,
                cancellationToken);

            if (!countryExists)
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.PhoneCountryNotFound);
            }

            var personPhones = await personPhoneRepository.FindAsync(
                x => x.PersonId == person.PersonId,
                cancellationToken);
            var primaryPersonPhone = personPhones
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.PersonPhoneId)
                .FirstOrDefault();
            var primaryPersonPhoneIdToExclude = primaryPersonPhone?.PersonPhoneId ?? 0;

            var phoneAlreadyExists = await personPhoneRepository.ExistsAsync(
                x => x.CountryId == request.PhoneCountryId.Value &&
                     x.PhoneNumber == phoneNumber &&
                     x.PersonPhoneId != primaryPersonPhoneIdToExclude,
                cancellationToken);

            if (phoneAlreadyExists)
            {
                return Result<AccountProfileDto>.Failure(AccountErrors.PhoneNumberAlreadyExists);
            }

            if (primaryPersonPhone is not null)
            {
                primaryPersonPhone.CountryId = request.PhoneCountryId.Value;
                primaryPersonPhone.PhoneNumber = phoneNumber;
                primaryPersonPhone.IsPrimary = true;

                personPhoneRepository.Update(primaryPersonPhone);
            }
            else
            {
                var newPrimaryPhone = new PersonPhone
                {
                    PersonId = person.PersonId,
                    CountryId = request.PhoneCountryId.Value,
                    PhoneNumber = phoneNumber,
                    IsPrimary = true
                };

                await personPhoneRepository.AddAsync(newPrimaryPhone, cancellationToken);
            }
        }

        person.FirstName = firstName;
        person.MiddleName = middleName;
        person.LastName = lastName;
        person.SecondLastName = secondLastName;
        person.BirthDate = birthDate;
        person.GenderId = genderId;
        person.AddressId = addressId;

        var personRepository = _unitOfWork.Repository<Person>();
        personRepository.Update(person);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var profile = await BuildProfileDtoAsync(user, person, cancellationToken);
        return Result<AccountProfileDto>.Success(profile);
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var currentPassword = NormalizeRequiredText(request?.CurrentPassword);
        var newPassword = NormalizeRequiredText(request?.NewPassword);

        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            return Result.Failure(AccountErrors.CurrentPasswordRequired);
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.Failure(AccountErrors.NewPasswordRequired);
        }

        if (newPassword.Length < MinPasswordLength)
        {
            return Result.Failure(AccountErrors.NewPasswordTooShort);
        }

        if (newPassword.Length > MaxPasswordLength)
        {
            return Result.Failure(AccountErrors.NewPasswordTooLong);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure(AccountErrors.UserInactiveInvalid);
        }

        if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
        {
            return Result.Failure(AccountErrors.CurrentPasswordInvalid);
        }

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<(User? User, Person? Person, Error? Error)> ResolveUserAndPersonAsync(
        int userId,
        int personId,
        bool validateActiveUser,
        CancellationToken cancellationToken)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return (null, null, AccountErrors.UserNotFound);
        }

        if (validateActiveUser && !user.IsActive)
        {
            return (null, null, AccountErrors.UserInactiveInvalid);
        }

        if (user.PersonId != personId)
        {
            return (null, null, AccountErrors.PersonNotFound);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var person = await personRepository.GetByIdAsync(personId, cancellationToken);

        if (person is null)
        {
            return (null, null, AccountErrors.PersonNotFound);
        }

        return (user, person, null);
    }

    private async Task<AccountProfileDto> BuildProfileDtoAsync(User user, Person person, CancellationToken cancellationToken)
    {
        var primaryEmail = await ResolvePrimaryEmailAsync(person.PersonId, cancellationToken);
        var primaryPhone = await ResolvePrimaryPhoneAsync(person.PersonId, cancellationToken);
        var roleNames = await GetActiveRoleNamesAsync(person.PersonId, cancellationToken);

        return new AccountProfileDto
        {
            UserId = user.UserId,
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
            PrimaryEmail = primaryEmail,
            PrimaryPhoneCountryId = primaryPhone.CountryId,
            PrimaryPhoneNumber = primaryPhone.PhoneNumber,
            IsActive = user.IsActive,
            Roles = roleNames
        };
    }

    private async Task<string?> ResolvePrimaryEmailAsync(int personId, CancellationToken cancellationToken)
    {
        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmails = await personEmailRepository.FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        var primaryPersonEmail = personEmails
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.PersonEmailId)
            .FirstOrDefault();

        if (primaryPersonEmail is null || string.IsNullOrWhiteSpace(primaryPersonEmail.EmailUser))
        {
            return null;
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomain = await emailDomainRepository.GetByIdAsync(primaryPersonEmail.EmailDomainId, cancellationToken);
        if (emailDomain is null || string.IsNullOrWhiteSpace(emailDomain.Domain))
        {
            return null;
        }

        return $"{primaryPersonEmail.EmailUser}@{emailDomain.Domain}";
    }

    private async Task<(int? CountryId, string? PhoneNumber)> ResolvePrimaryPhoneAsync(int personId, CancellationToken cancellationToken)
    {
        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        var personPhones = await personPhoneRepository.FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        var primaryPhone = personPhones
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.PersonPhoneId)
            .FirstOrDefault();

        if (primaryPhone is null)
        {
            return (null, null);
        }

        return (primaryPhone.CountryId, primaryPhone.PhoneNumber);
    }

    private async Task<IReadOnlyList<string>> GetActiveRoleNamesAsync(int personId, CancellationToken cancellationToken)
    {
        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRoles = await personRoleRepository.FindAsync(
            x => x.PersonId == personId && x.IsActive,
            cancellationToken);

        if (personRoles.Count == 0)
        {
            return Array.Empty<string>();
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roleNames = new List<string>();

        foreach (var personRole in personRoles)
        {
            var role = await roleRepository.GetByIdAsync(personRole.RoleId, cancellationToken);
            if (role is not null && !string.IsNullOrWhiteSpace(role.RoleName))
            {
                roleNames.Add(role.RoleName);
            }
        }

        return roleNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Error? ValidateProfileFields(
        string firstName,
        string? middleName,
        string lastName,
        string? secondLastName,
        DateTime? birthDate,
        int? genderId,
        int? addressId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return AccountErrors.FirstNameRequired;
        }

        if (firstName.Length > FirstNameMaxLength)
        {
            return AccountErrors.FirstNameTooLong;
        }

        if (middleName is not null && middleName.Length > MiddleNameMaxLength)
        {
            return AccountErrors.MiddleNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return AccountErrors.LastNameRequired;
        }

        if (lastName.Length > LastNameMaxLength)
        {
            return AccountErrors.LastNameTooLong;
        }

        if (secondLastName is not null && secondLastName.Length > SecondLastNameMaxLength)
        {
            return AccountErrors.SecondLastNameTooLong;
        }

        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
        {
            return AccountErrors.BirthDateInvalid;
        }

        if (genderId.HasValue && genderId.Value <= 0)
        {
            return AccountErrors.GenderIdInvalid;
        }

        if (addressId.HasValue && addressId.Value <= 0)
        {
            return AccountErrors.AddressIdInvalid;
        }

        return null;
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string NormalizeRequiredText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool TrySplitEmail(string email, out string emailUser, out string domain)
    {
        emailUser = string.Empty;
        domain = string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return false;
        }

        if (atIndex != email.LastIndexOf('@'))
        {
            return false;
        }

        if (atIndex >= email.Length - 1)
        {
            return false;
        }

        emailUser = email[..atIndex];
        domain = email[(atIndex + 1)..];

        if (string.IsNullOrWhiteSpace(emailUser) || string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        if (!domain.Contains('.'))
        {
            return false;
        }

        return true;
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
