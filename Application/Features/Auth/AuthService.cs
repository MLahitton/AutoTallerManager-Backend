using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Common.Security;
using Application.Features.Auth.Dtos;
using Application.Features.Auth.Errors;
using Application.Features.Auth.Requests;
using Domain.Entities;

namespace Application.Features.Auth;

public class AuthService : IAuthService
{
    private const int DocumentNumberMaxLength = 30;
    private const int FirstNameMaxLength = 50;
    private const int MiddleNameMaxLength = 50;
    private const int LastNameMaxLength = 50;
    private const int SecondLastNameMaxLength = 50;
    private const int PhoneNumberMaxLength = 20;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;
    private const string ClientRoleName = "Client";
    private const string LoginAuditActionTypeName = "LOGIN";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string UserEntityName = "User";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IAuthTokenSettings _authTokenSettings;
    private readonly IAuditLogger _auditLogger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAuthTokenSettings authTokenSettings,
        IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _authTokenSettings = authTokenSettings;
        _auditLogger = auditLogger;
    }

    public async Task<Result<AuthResponseDto>> RegisterClientAsync(RegisterClientRequest request, CancellationToken cancellationToken = default)
    {
        var documentTypeId = request?.DocumentTypeId ?? 0;
        var documentNumber = NormalizeRequiredText(request?.DocumentNumber);
        var firstName = NormalizeRequiredText(request?.FirstName);
        var middleName = NormalizeOptionalText(request?.MiddleName);
        var lastName = NormalizeRequiredText(request?.LastName);
        var secondLastName = NormalizeOptionalText(request?.SecondLastName);
        var birthDate = request?.BirthDate;
        var genderId = request?.GenderId;
        var addressId = request?.AddressId;
        var normalizedEmail = NormalizeEmail(request?.Email);
        var password = NormalizePassword(request?.Password);
        var phoneNumberProvided = request?.PhoneNumber is not null;
        var normalizedPhoneNumber = NormalizeOptionalText(request?.PhoneNumber);
        var phoneCountryId = request?.PhoneCountryId;

        var validationError = ValidateRegisterClientInput(
            documentTypeId,
            documentNumber,
            firstName,
            middleName,
            lastName,
            secondLastName,
            birthDate,
            genderId,
            addressId,
            normalizedEmail,
            password,
            phoneNumberProvided,
            normalizedPhoneNumber,
            phoneCountryId);

        if (validationError is not null)
        {
            return Result<AuthResponseDto>.Failure(validationError);
        }

        if (!TrySplitEmail(normalizedEmail, out var emailUser, out var domain))
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.EmailInvalid);
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypeExists = await documentTypeRepository.ExistsAsync(
            x => x.DocumentTypeId == documentTypeId,
            cancellationToken);

        if (!documentTypeExists)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.DocumentTypeNotFound);
        }

        if (genderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == genderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<AuthResponseDto>.Failure(AuthErrors.GenderNotFound);
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
                return Result<AuthResponseDto>.Failure(AuthErrors.AddressNotFound);
            }
        }

        if (phoneNumberProvided)
        {
            var countryRepository = _unitOfWork.Repository<Country>();
            var countryExists = await countryRepository.ExistsAsync(
                x => x.CountryId == phoneCountryId!.Value,
                cancellationToken);

            if (!countryExists)
            {
                return Result<AuthResponseDto>.Failure(AuthErrors.PhoneCountryNotFound);
            }
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var documentNumberExists = await personRepository.ExistsAsync(
            x => x.DocumentNumber == documentNumber,
            cancellationToken);

        if (documentNumberExists)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.DocumentNumberAlreadyExists);
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var clientRole = roles.FirstOrDefault(
            x => x.RoleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase));

        if (clientRole is null)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.ClientRoleNotFound);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var existingEmailDomains = await emailDomainRepository.FindAsync(
            x => x.Domain == domain,
            cancellationToken);
        var emailDomain = existingEmailDomains.FirstOrDefault();

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        if (emailDomain is not null)
        {
            var emailAlreadyExists = await personEmailRepository.ExistsAsync(
                x => x.EmailUser == emailUser && x.EmailDomainId == emailDomain.EmailDomainId,
                cancellationToken);

            if (emailAlreadyExists)
            {
                return Result<AuthResponseDto>.Failure(AuthErrors.EmailAlreadyExists);
            }
        }

        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        if (phoneNumberProvided)
        {
            var phoneAlreadyExists = await personPhoneRepository.ExistsAsync(
                x => x.CountryId == phoneCountryId!.Value && x.PhoneNumber == normalizedPhoneNumber!,
                cancellationToken);

            if (phoneAlreadyExists)
            {
                return Result<AuthResponseDto>.Failure(AuthErrors.PhoneNumberAlreadyExists);
            }
        }

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(GetSafeRefreshDays());

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

        if (emailDomain is null)
        {
            emailDomain = new EmailDomain
            {
                Domain = domain
            };

            await emailDomainRepository.AddAsync(emailDomain, cancellationToken);
        }

        var personEmail = new PersonEmail
        {
            Person = person,
            EmailUser = emailUser,
            IsPrimary = true
        };

        if (emailDomain.EmailDomainId > 0)
        {
            personEmail.EmailDomainId = emailDomain.EmailDomainId;
        }
        else
        {
            personEmail.EmailDomain = emailDomain;
        }

        var personRole = new PersonRole
        {
            Person = person,
            RoleId = clientRole.RoleId,
            IsActive = true
        };

        var user = new User
        {
            Person = person,
            PasswordHash = _passwordHasher.Hash(password),
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiresAt,
            IsActive = true
        };

        await personRepository.AddAsync(person, cancellationToken);
        await personEmailRepository.AddAsync(personEmail, cancellationToken);

        if (phoneNumberProvided)
        {
            var personPhone = new PersonPhone
            {
                Person = person,
                CountryId = phoneCountryId!.Value,
                PhoneNumber = normalizedPhoneNumber!,
                IsPrimary = true
            };

            await personPhoneRepository.AddAsync(personPhone, cancellationToken);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        await personRoleRepository.AddAsync(personRole, cancellationToken);

        var userRepository = _unitOfWork.Repository<User>();
        await userRepository.AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roleNames = new List<string> { clientRole.RoleName };
        var tokenResult = _jwtTokenGenerator.GenerateToken(user.UserId, person.PersonId, normalizedEmail, roleNames);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            AccessTokenExpiresAt = tokenResult.ExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new AuthUserDto
            {
                UserId = user.UserId,
                PersonId = person.PersonId,
                Email = normalizedEmail,
                IsActive = user.IsActive,
                Roles = roleNames
            }
        });
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request?.Email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.EmailRequired);
        }

        if (!TrySplitEmail(normalizedEmail, out var emailUser, out var domain))
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.EmailInvalid);
        }

        var password = NormalizePassword(request?.Password);
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.PasswordRequired);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomains = await emailDomainRepository.FindAsync(
            x => x.Domain == domain,
            cancellationToken);

        var emailDomain = emailDomains.FirstOrDefault();
        if (emailDomain is null)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.InvalidCredentials);
        }

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmails = await personEmailRepository.FindAsync(
            x => x.EmailUser == emailUser && x.EmailDomainId == emailDomain.EmailDomainId,
            cancellationToken);

        var personEmail = personEmails.FirstOrDefault();
        if (personEmail is null)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.InvalidCredentials);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.FindAsync(
            x => x.PersonId == personEmail.PersonId,
            cancellationToken);

        var user = users.FirstOrDefault();
        if (user is null)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.UserInactiveInvalid);
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.InvalidCredentials);
        }

        var roleNames = await GetActiveRoleNamesAsync(user.PersonId, cancellationToken);
        var tokenResult = _jwtTokenGenerator.GenerateToken(user.UserId, user.PersonId, normalizedEmail, roleNames);

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(GetSafeRefreshDays());

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = refreshTokenExpiresAt;

        userRepository.Update(user);

        await _auditLogger.LogAsync(
            user.UserId,
            LoginAuditActionTypeName,
            UserEntityName,
            user.UserId,
            $"User {user.UserId} logged in successfully.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            AccessTokenExpiresAt = tokenResult.ExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new AuthUserDto
            {
                UserId = user.UserId,
                PersonId = user.PersonId,
                Email = normalizedEmail,
                IsActive = user.IsActive,
                Roles = roleNames
            }
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = NormalizeRefreshToken(request?.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.RefreshTokenRequired);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.FindAsync(
            x => x.RefreshToken == refreshToken,
            cancellationToken);

        var user = users.FirstOrDefault();
        if (user is null)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.RefreshTokenInvalid);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.UserInactiveInvalid);
        }

        if (!user.RefreshTokenExpiration.HasValue || user.RefreshTokenExpiration.Value <= DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Failure(AuthErrors.RefreshTokenExpired);
        }

        var resolvedEmailResult = await ResolveUserEmailAsync(user.PersonId, cancellationToken);
        if (resolvedEmailResult.IsFailure)
        {
            return Result<AuthResponseDto>.Failure(resolvedEmailResult.Error);
        }

        var email = resolvedEmailResult.Value!;
        var roleNames = await GetActiveRoleNamesAsync(user.PersonId, cancellationToken);
        var tokenResult = _jwtTokenGenerator.GenerateToken(user.UserId, user.PersonId, email, roleNames);

        var newRefreshToken = _refreshTokenGenerator.Generate();
        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(GetSafeRefreshDays());

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiration = newRefreshTokenExpiresAt;

        userRepository.Update(user);

        await _auditLogger.LogAsync(
            user.UserId,
            LoginAuditActionTypeName,
            UserEntityName,
            user.UserId,
            $"User {user.UserId} refreshed authentication token.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            AccessTokenExpiresAt = tokenResult.ExpiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newRefreshTokenExpiresAt,
            User = new AuthUserDto
            {
                UserId = user.UserId,
                PersonId = user.PersonId,
                Email = email,
                IsActive = user.IsActive,
                Roles = roleNames
            }
        });
    }

    public async Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = NormalizeRefreshToken(request?.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure(AuthErrors.RefreshTokenRequired);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.FindAsync(
            x => x.RefreshToken == refreshToken,
            cancellationToken);

        var user = users.FirstOrDefault();
        if (user is null)
        {
            return Result.Failure(AuthErrors.RefreshTokenInvalid);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiration = null;

        userRepository.Update(user);

        await _auditLogger.LogAsync(
            user.UserId,
            UpdateAuditActionTypeName,
            UserEntityName,
            user.UserId,
            $"User {user.UserId} logged out.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<string>> ResolveUserEmailAsync(int personId, CancellationToken cancellationToken)
    {
        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var personEmails = await personEmailRepository.FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        var personEmail = personEmails
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.PersonEmailId)
            .FirstOrDefault();

        if (personEmail is null || string.IsNullOrWhiteSpace(personEmail.EmailUser))
        {
            return Result<string>.Failure(AuthErrors.UserEmailNotFound);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomain = await emailDomainRepository.GetByIdAsync(personEmail.EmailDomainId, cancellationToken);

        if (emailDomain is null || string.IsNullOrWhiteSpace(emailDomain.Domain))
        {
            return Result<string>.Failure(AuthErrors.UserEmailNotFound);
        }

        return Result<string>.Success($"{personEmail.EmailUser}@{emailDomain.Domain}");
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

    private int GetSafeRefreshDays()
    {
        return _authTokenSettings.RefreshTokenExpirationDays > 0
            ? _authTokenSettings.RefreshTokenExpirationDays
            : 7;
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string NormalizePassword(string? password)
    {
        return (password ?? string.Empty).Trim();
    }

    private static string NormalizeRefreshToken(string? refreshToken)
    {
        return (refreshToken ?? string.Empty).Trim();
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

    private static Error? ValidateRegisterClientInput(
        int documentTypeId,
        string documentNumber,
        string firstName,
        string? middleName,
        string lastName,
        string? secondLastName,
        DateTime? birthDate,
        int? genderId,
        int? addressId,
        string normalizedEmail,
        string password,
        bool phoneNumberProvided,
        string? phoneNumber,
        int? phoneCountryId)
    {
        if (documentTypeId <= 0)
        {
            return AuthErrors.DocumentTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return AuthErrors.DocumentNumberRequired;
        }

        if (documentNumber.Length > DocumentNumberMaxLength)
        {
            return AuthErrors.DocumentNumberTooLong;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return AuthErrors.FirstNameRequired;
        }

        if (firstName.Length > FirstNameMaxLength)
        {
            return AuthErrors.FirstNameTooLong;
        }

        if (middleName is not null && middleName.Length > MiddleNameMaxLength)
        {
            return AuthErrors.MiddleNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return AuthErrors.LastNameRequired;
        }

        if (lastName.Length > LastNameMaxLength)
        {
            return AuthErrors.LastNameTooLong;
        }

        if (secondLastName is not null && secondLastName.Length > SecondLastNameMaxLength)
        {
            return AuthErrors.SecondLastNameTooLong;
        }

        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
        {
            return AuthErrors.BirthDateInvalid;
        }

        if (genderId.HasValue && genderId.Value <= 0)
        {
            return AuthErrors.GenderIdInvalid;
        }

        if (addressId.HasValue && addressId.Value <= 0)
        {
            return AuthErrors.AddressIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return AuthErrors.EmailRequired;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthErrors.PasswordRequired;
        }

        if (password.Length < MinPasswordLength)
        {
            return AuthErrors.PasswordTooShort;
        }

        if (password.Length > MaxPasswordLength)
        {
            return AuthErrors.PasswordTooLong;
        }

        if (phoneNumberProvided)
        {
            if (!phoneCountryId.HasValue || phoneCountryId.Value <= 0)
            {
                return AuthErrors.PhoneCountryIdRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return AuthErrors.PhoneNumberInvalid;
            }

            if (phoneNumber.Length > PhoneNumberMaxLength)
            {
                return AuthErrors.PhoneNumberTooLong;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                return AuthErrors.PhoneNumberInvalid;
            }
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
}
