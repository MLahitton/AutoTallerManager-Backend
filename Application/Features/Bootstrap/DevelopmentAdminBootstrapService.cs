using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Common.Security;
using Domain.Entities;

namespace Application.Features.Bootstrap;

public sealed class DevelopmentAdminBootstrapService : IDevelopmentAdminBootstrapService
{
    private const int MinPasswordLength = 8;
    private const string AdminRoleName = "Admin";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public DevelopmentAdminBootstrapService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<BootstrapAdminResultDto>> EnsureBootstrapAdminAsync(
        BootstrapAdminSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (!settings.Enabled)
        {
            return Result<BootstrapAdminResultDto>.Success(new BootstrapAdminResultDto
            {
                Created = false,
                Skipped = true,
                Message = "Bootstrap admin is disabled."
            });
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var adminRole = roles.FirstOrDefault(
            x => x.RoleName.Equals(AdminRoleName, StringComparison.OrdinalIgnoreCase));

        if (adminRole is null)
        {
            return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.AdminRoleNotFound);
        }

        if (await HasActiveAdminAsync(adminRole.RoleId, cancellationToken))
        {
            return Result<BootstrapAdminResultDto>.Success(new BootstrapAdminResultDto
            {
                Created = false,
                Skipped = true,
                Message = "Active admin already exists."
            });
        }

        var normalizedEmail = NormalizeEmail(settings.Email);
        var password = NormalizeRequiredText(settings.Password);
        var documentNumber = NormalizeRequiredText(settings.DocumentNumber);
        var firstName = NormalizeRequiredText(settings.FirstName);
        var middleName = NormalizeOptionalText(settings.MiddleName);
        var lastName = NormalizeRequiredText(settings.LastName);
        var secondLastName = NormalizeOptionalText(settings.SecondLastName);

        var validationError = ValidateSettings(
            settings.DocumentTypeId,
            documentNumber,
            firstName,
            lastName,
            normalizedEmail,
            password);

        if (validationError is not null)
        {
            return Result<BootstrapAdminResultDto>.Failure(validationError);
        }

        if (!TrySplitEmail(normalizedEmail, out var emailUser, out var domain))
        {
            return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.EmailInvalid);
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypeExists = await documentTypeRepository.ExistsAsync(
            x => x.DocumentTypeId == settings.DocumentTypeId,
            cancellationToken);

        if (!documentTypeExists)
        {
            return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.DocumentTypeNotFound);
        }

        if (settings.GenderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == settings.GenderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.GenderNotFound);
            }
        }

        if (settings.AddressId.HasValue)
        {
            var addressRepository = _unitOfWork.Repository<Address>();
            var addressExists = await addressRepository.ExistsAsync(
                x => x.AddressId == settings.AddressId.Value,
                cancellationToken);

            if (!addressExists)
            {
                return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.AddressNotFound);
            }
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var documentNumberExists = await personRepository.ExistsAsync(
            x => x.DocumentNumber == documentNumber,
            cancellationToken);

        if (documentNumberExists)
        {
            return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.DocumentNumberAlreadyExists);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var allEmailDomains = await emailDomainRepository.GetAllAsync(cancellationToken);
        var emailDomain = allEmailDomains.FirstOrDefault(
            x => x.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        if (emailDomain is not null)
        {
            var emailAlreadyExists = await personEmailRepository.ExistsAsync(
                x => x.EmailUser == emailUser && x.EmailDomainId == emailDomain.EmailDomainId,
                cancellationToken);

            if (emailAlreadyExists)
            {
                return Result<BootstrapAdminResultDto>.Failure(BootstrapAdminErrors.EmailAlreadyExists);
            }
        }

        var person = new Person
        {
            DocumentTypeId = settings.DocumentTypeId,
            DocumentNumber = documentNumber,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            SecondLastName = secondLastName,
            BirthDate = settings.BirthDate,
            GenderId = settings.GenderId,
            AddressId = settings.AddressId
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
            RoleId = adminRole.RoleId,
            IsActive = true
        };

        var user = new User
        {
            Person = person,
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = true,
            RefreshToken = null,
            RefreshTokenExpiration = null
        };

        await personRepository.AddAsync(person, cancellationToken);
        await personEmailRepository.AddAsync(personEmail, cancellationToken);
        await _unitOfWork.Repository<PersonRole>().AddAsync(personRole, cancellationToken);
        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BootstrapAdminResultDto>.Success(new BootstrapAdminResultDto
        {
            Created = true,
            Skipped = false,
            UserId = user.UserId,
            PersonId = person.PersonId,
            Email = normalizedEmail,
            Message = "Bootstrap admin created successfully."
        });
    }

    private async Task<bool> HasActiveAdminAsync(int adminRoleId, CancellationToken cancellationToken)
    {
        var personRoles = await _unitOfWork.Repository<PersonRole>().FindAsync(
            x => x.RoleId == adminRoleId && x.IsActive,
            cancellationToken);

        if (personRoles.Count == 0)
        {
            return false;
        }

        var adminPersonIds = personRoles
            .Select(x => x.PersonId)
            .Distinct()
            .ToList();

        if (adminPersonIds.Count == 0)
        {
            return false;
        }

        return await _unitOfWork.Repository<User>().ExistsAsync(
            x => adminPersonIds.Contains(x.PersonId) && x.IsActive,
            cancellationToken);
    }

    private static Error? ValidateSettings(
        int documentTypeId,
        string documentNumber,
        string firstName,
        string lastName,
        string email,
        string password)
    {
        if (documentTypeId <= 0)
        {
            return BootstrapAdminErrors.DocumentTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return BootstrapAdminErrors.DocumentNumberRequired;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return BootstrapAdminErrors.FirstNameRequired;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return BootstrapAdminErrors.LastNameRequired;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BootstrapAdminErrors.EmailRequired;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return BootstrapAdminErrors.PasswordRequired;
        }

        if (password.Length < MinPasswordLength)
        {
            return BootstrapAdminErrors.PasswordTooShort;
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
        if (atIndex <= 0 || atIndex != email.LastIndexOf('@') || atIndex >= email.Length - 1)
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
