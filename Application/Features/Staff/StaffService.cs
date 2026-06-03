using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Common.Security;
using Application.Features.Staff.Dtos;
using Application.Features.Staff.Errors;
using Application.Features.Staff.Requests;
using Domain.Entities;

namespace Application.Features.Staff;

public class StaffService : IStaffService
{
    private const int DocumentNumberMaxLength = 30;
    private const int FirstNameMaxLength = 50;
    private const int MiddleNameMaxLength = 50;
    private const int LastNameMaxLength = 50;
    private const int SecondLastNameMaxLength = 50;
    private const int EmailUserMaxLength = 100;
    private const int PhoneNumberMaxLength = 20;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;

    private const string AdminRoleName = "Admin";
    private const string ReceptionistRoleName = "Receptionist";
    private const string MechanicRoleName = "Mechanic";
    private const string ClientRoleName = "Client";

    private static readonly HashSet<string> AllowedStaffRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        AdminRoleName,
        ReceptionistRoleName,
        MechanicRoleName
    };
    private const string CreateAuditActionTypeName = "CREATE";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string UserEntityName = "User";
    private const string PersonRoleEntityName = "PersonRole";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public StaffService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<Result<StaffUserDto>> RegisterStaffAsync(RegisterStaffRequest request, int currentUserId, CancellationToken cancellationToken = default)
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
        var password = NormalizeRequiredText(request?.Password);
        var normalizedRoleName = NormalizeRequiredText(request?.RoleName);
        var phoneNumberProvided = request?.PhoneNumber is not null;
        var phoneNumber = NormalizeOptionalText(request?.PhoneNumber);
        var phoneCountryId = request?.PhoneCountryId;
        var specialtyIdsFromRequest = request?.SpecialtyIds?.ToList() ?? new List<int>();

        var validationError = ValidateRegisterStaffInput(
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
            normalizedRoleName,
            phoneNumberProvided,
            phoneCountryId,
            phoneNumber,
            specialtyIdsFromRequest);

        if (validationError is not null)
        {
            return Result<StaffUserDto>.Failure(validationError);
        }

        if (!TrySplitEmail(normalizedEmail, out var emailUser, out var domain))
        {
            return Result<StaffUserDto>.Failure(StaffErrors.EmailInvalid);
        }

        if (emailUser.Length > EmailUserMaxLength)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.EmailInvalid);
        }

        var normalizedSpecialtyIds = new List<int>();
        if (normalizedRoleName.Equals(MechanicRoleName, StringComparison.OrdinalIgnoreCase))
        {
            if (specialtyIdsFromRequest.Count != specialtyIdsFromRequest.Distinct().Count())
            {
                return Result<StaffUserDto>.Failure(StaffErrors.DuplicateSpecialtyConflict);
            }

            normalizedSpecialtyIds = specialtyIdsFromRequest.Distinct().ToList();

            if (normalizedSpecialtyIds.Any(x => x <= 0))
            {
                return Result<StaffUserDto>.Failure(StaffErrors.SpecialtyIdInvalid);
            }
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypeExists = await documentTypeRepository.ExistsAsync(
            x => x.DocumentTypeId == documentTypeId,
            cancellationToken);

        if (!documentTypeExists)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.DocumentTypeNotFound);
        }

        if (genderId.HasValue)
        {
            var genderRepository = _unitOfWork.Repository<Gender>();
            var genderExists = await genderRepository.ExistsAsync(
                x => x.GenderId == genderId.Value,
                cancellationToken);

            if (!genderExists)
            {
                return Result<StaffUserDto>.Failure(StaffErrors.GenderNotFound);
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
                return Result<StaffUserDto>.Failure(StaffErrors.AddressNotFound);
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
                return Result<StaffUserDto>.Failure(StaffErrors.PhoneCountryNotFound);
            }
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var documentNumberExists = await personRepository.ExistsAsync(
            x => x.DocumentNumber == documentNumber,
            cancellationToken);

        if (documentNumberExists)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.DocumentNumberAlreadyExists);
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var targetRole = roles.FirstOrDefault(
            x => x.RoleName.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));

        if (targetRole is null)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.StaffRoleNotFound);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomains = await emailDomainRepository.FindAsync(
            x => x.Domain == domain,
            cancellationToken);
        var emailDomain = emailDomains.FirstOrDefault();

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        if (emailDomain is not null)
        {
            var emailAlreadyExists = await personEmailRepository.ExistsAsync(
                x => x.EmailUser == emailUser && x.EmailDomainId == emailDomain.EmailDomainId,
                cancellationToken);

            if (emailAlreadyExists)
            {
                return Result<StaffUserDto>.Failure(StaffErrors.EmailAlreadyExists);
            }
        }

        var personPhoneRepository = _unitOfWork.Repository<PersonPhone>();
        if (phoneNumberProvided)
        {
            var phoneAlreadyExists = await personPhoneRepository.ExistsAsync(
                x => x.CountryId == phoneCountryId!.Value && x.PhoneNumber == phoneNumber!,
                cancellationToken);

            if (phoneAlreadyExists)
            {
                return Result<StaffUserDto>.Failure(StaffErrors.PhoneNumberAlreadyExists);
            }
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        if (normalizedRoleName.Equals(MechanicRoleName, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var specialtyId in normalizedSpecialtyIds)
            {
                var specialtyExists = await specialtyRepository.ExistsAsync(
                    x => x.SpecialtyId == specialtyId,
                    cancellationToken);

                if (!specialtyExists)
                {
                    return Result<StaffUserDto>.Failure(StaffErrors.SpecialtyNotFound);
                }
            }
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
            RoleId = targetRole.RoleId,
            IsActive = true
        };

        var user = new User
        {
            Person = person,
            PasswordHash = _passwordHasher.Hash(password),
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
                PhoneNumber = phoneNumber!,
                IsPrimary = true
            };

            await personPhoneRepository.AddAsync(personPhone, cancellationToken);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        await personRoleRepository.AddAsync(personRole, cancellationToken);

        var userRepository = _unitOfWork.Repository<User>();
        await userRepository.AddAsync(user, cancellationToken);

        if (normalizedRoleName.Equals(MechanicRoleName, StringComparison.OrdinalIgnoreCase))
        {
            var specialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
            foreach (var specialtyId in normalizedSpecialtyIds)
            {
                var assignment = new MechanicSpecialtyAssignment
                {
                    Person = person,
                    SpecialtyId = specialtyId
                };

                await specialtyAssignmentRepository.AddAsync(assignment, cancellationToken);
            }
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                UserEntityName,
                user.UserId,
                $"User {user.UserId} created.",
                transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                PersonRoleEntityName,
                personRole.PersonRoleId,
                $"Role {personRole.RoleId} assigned to person {person.PersonId}.",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var staffUserDto = await BuildStaffUserDtoAsync(user, transactionCancellationToken);
            return Result<StaffUserDto>.Success(staffUserDto);
        }, cancellationToken);
    }

    public async Task<Result<StaffUserDto>> ActivateUserAsync(int userId, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.UserIdInvalid);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            user.IsActive = true;
            userRepository.Update(user);

            await _auditLogger.LogAsync(
                currentUserId,
                UpdateAuditActionTypeName,
                UserEntityName,
                user.UserId,
                $"User {user.UserId} status changed.",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var staffUserDto = await BuildStaffUserDtoAsync(user, cancellationToken);
        return Result<StaffUserDto>.Success(staffUserDto);
    }

    public async Task<Result<StaffUserDto>> DeactivateUserAsync(int userId, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || currentUserId <= 0)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.UserIdInvalid);
        }

        if (userId == currentUserId)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.CannotDeactivateCurrentUserConflict);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<StaffUserDto>.Failure(StaffErrors.UserNotFound);
        }

        var requiresUpdate = false;

        if (user.IsActive)
        {
            user.IsActive = false;
            requiresUpdate = true;
        }

        if (user.RefreshToken is not null || user.RefreshTokenExpiration.HasValue)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiration = null;
            requiresUpdate = true;
        }

        if (requiresUpdate)
        {
            userRepository.Update(user);

            await _auditLogger.LogAsync(
                currentUserId,
                UpdateAuditActionTypeName,
                UserEntityName,
                user.UserId,
                $"User {user.UserId} status changed.",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var staffUserDto = await BuildStaffUserDtoAsync(user, cancellationToken);
        return Result<StaffUserDto>.Success(staffUserDto);
    }

    public async Task<Result<IReadOnlyList<MechanicSpecialtySummaryDto>>> GetMechanicSpecialtiesAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var personValidationError = await ValidateMechanicPersonAsync(personId, cancellationToken);
        if (personValidationError is not null)
        {
            return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(personValidationError);
        }

        var specialties = await GetMechanicSpecialtySummariesAsync(personId, cancellationToken);
        return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Success(specialties);
    }

    public async Task<Result<IReadOnlyList<MechanicSpecialtySummaryDto>>> ReplaceMechanicSpecialtiesAsync(
        int personId,
        ReplaceMechanicSpecialtiesRequest request,
        CancellationToken cancellationToken = default)
    {
        var personValidationError = await ValidateMechanicPersonAsync(personId, cancellationToken);
        if (personValidationError is not null)
        {
            return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(personValidationError);
        }

        if (request?.SpecialtyIds is null)
        {
            return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(StaffErrors.SpecialtyIdInvalid);
        }

        var requestedSpecialtyIds = request.SpecialtyIds.ToList();
        if (requestedSpecialtyIds.Count != requestedSpecialtyIds.Distinct().Count())
        {
            return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(StaffErrors.DuplicateSpecialtyConflict);
        }

        if (requestedSpecialtyIds.Any(x => x <= 0))
        {
            return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(StaffErrors.SpecialtyIdInvalid);
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        foreach (var specialtyId in requestedSpecialtyIds)
        {
            var specialtyExists = await specialtyRepository.ExistsAsync(
                x => x.SpecialtyId == specialtyId,
                cancellationToken);

            if (!specialtyExists)
            {
                return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(StaffErrors.SpecialtyNotFound);
            }
        }

        var requestedSet = requestedSpecialtyIds.ToHashSet();
        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var currentAssignments = await assignmentRepository.FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        var currentSet = currentAssignments.Select(x => x.SpecialtyId).ToHashSet();
        var specialtiesToAdd = requestedSet.Except(currentSet).ToList();
        var assignmentsToRemove = currentAssignments.Where(x => !requestedSet.Contains(x.SpecialtyId)).ToList();

        if (assignmentsToRemove.Count > 0)
        {
            var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
            foreach (var assignmentToRemove in assignmentsToRemove)
            {
                var inUse = await mechanicAssignmentRepository.ExistsAsync(
                    x => x.MechanicPersonId == personId && x.SpecialtyId == assignmentToRemove.SpecialtyId,
                    cancellationToken);

                if (inUse)
                {
                    return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Failure(StaffErrors.MechanicSpecialtyInUseConflict);
                }
            }
        }

        foreach (var assignmentToRemove in assignmentsToRemove)
        {
            assignmentRepository.Remove(assignmentToRemove);
        }

        foreach (var specialtyIdToAdd in specialtiesToAdd)
        {
            var newAssignment = new MechanicSpecialtyAssignment
            {
                PersonId = personId,
                SpecialtyId = specialtyIdToAdd
            };

            await assignmentRepository.AddAsync(newAssignment, cancellationToken);
        }

        if (assignmentsToRemove.Count > 0 || specialtiesToAdd.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var specialties = await GetMechanicSpecialtySummariesAsync(personId, cancellationToken);
        return Result<IReadOnlyList<MechanicSpecialtySummaryDto>>.Success(specialties);
    }

    private async Task<StaffUserDto> BuildStaffUserDtoAsync(User user, CancellationToken cancellationToken)
    {
        var email = await ResolvePrimaryEmailAsync(user.PersonId, cancellationToken) ?? string.Empty;
        var roleNames = await GetActiveRoleNamesAsync(user.PersonId, cancellationToken);
        var specialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var specialtyIds = await specialtyAssignmentRepository.FindAsync(
            x => x.PersonId == user.PersonId,
            cancellationToken);

        return new StaffUserDto
        {
            UserId = user.UserId,
            PersonId = user.PersonId,
            Email = email,
            RoleName = roleNames.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            SpecialtyIds = specialtyIds
                .Select(x => x.SpecialtyId)
                .Distinct()
                .OrderBy(x => x)
                .ToList()
        };
    }

    private async Task<Error?> ValidateMechanicPersonAsync(int personId, CancellationToken cancellationToken)
    {
        if (personId <= 0)
        {
            return StaffErrors.PersonIdInvalid;
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return StaffErrors.PersonNotFound;
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return StaffErrors.PersonIsNotMechanicInvalid;
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var hasActiveMechanicRole = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken);

        if (!hasActiveMechanicRole)
        {
            return StaffErrors.PersonIsNotMechanicInvalid;
        }

        return null;
    }

    private async Task<IReadOnlyList<MechanicSpecialtySummaryDto>> GetMechanicSpecialtySummariesAsync(
        int personId,
        CancellationToken cancellationToken)
    {
        var assignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var assignments = await assignmentRepository.FindAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (assignments.Count == 0)
        {
            return Array.Empty<MechanicSpecialtySummaryDto>();
        }

        var specialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var specialties = await specialtyRepository.GetAllAsync(cancellationToken);
        var specialtyNameById = specialties.ToDictionary(x => x.SpecialtyId, x => x.Name);

        return assignments
            .OrderBy(x => x.AssignmentId)
            .Select(x => new MechanicSpecialtySummaryDto
            {
                AssignmentId = x.AssignmentId,
                SpecialtyId = x.SpecialtyId,
                SpecialtyName = specialtyNameById.TryGetValue(x.SpecialtyId, out var specialtyName)
                    ? specialtyName
                    : string.Empty
            })
            .ToList();
    }

    private async Task<int?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);

        return roles
            .Where(x => x.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
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

        foreach (var personRole in personRoles.OrderBy(x => x.PersonRoleId))
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

    private static Error? ValidateRegisterStaffInput(
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
        string normalizedRoleName,
        bool phoneNumberProvided,
        int? phoneCountryId,
        string? phoneNumber,
        IReadOnlyList<int> specialtyIds)
    {
        if (documentTypeId <= 0)
        {
            return StaffErrors.DocumentTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return StaffErrors.DocumentNumberRequired;
        }

        if (documentNumber.Length > DocumentNumberMaxLength)
        {
            return StaffErrors.DocumentNumberTooLong;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return StaffErrors.FirstNameRequired;
        }

        if (firstName.Length > FirstNameMaxLength)
        {
            return StaffErrors.FirstNameTooLong;
        }

        if (middleName is not null && middleName.Length > MiddleNameMaxLength)
        {
            return StaffErrors.MiddleNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return StaffErrors.LastNameRequired;
        }

        if (lastName.Length > LastNameMaxLength)
        {
            return StaffErrors.LastNameTooLong;
        }

        if (secondLastName is not null && secondLastName.Length > SecondLastNameMaxLength)
        {
            return StaffErrors.SecondLastNameTooLong;
        }

        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
        {
            return StaffErrors.BirthDateInvalid;
        }

        if (genderId.HasValue && genderId.Value <= 0)
        {
            return StaffErrors.GenderIdInvalid;
        }

        if (addressId.HasValue && addressId.Value <= 0)
        {
            return StaffErrors.AddressIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return StaffErrors.EmailRequired;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return StaffErrors.PasswordRequired;
        }

        if (password.Length < MinPasswordLength)
        {
            return StaffErrors.PasswordTooShort;
        }

        if (password.Length > MaxPasswordLength)
        {
            return StaffErrors.PasswordTooLong;
        }

        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            return StaffErrors.RoleNameRequired;
        }

        if (normalizedRoleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return StaffErrors.StaffRoleInvalid;
        }

        if (!AllowedStaffRoles.Contains(normalizedRoleName))
        {
            return StaffErrors.StaffRoleInvalid;
        }

        if (!normalizedRoleName.Equals(MechanicRoleName, StringComparison.OrdinalIgnoreCase) && specialtyIds.Count > 0)
        {
            return StaffErrors.SpecialtiesOnlyAllowedForMechanicInvalid;
        }

        if (phoneNumberProvided)
        {
            if (!phoneCountryId.HasValue || phoneCountryId.Value <= 0)
            {
                return StaffErrors.PhoneCountryIdRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return StaffErrors.PhoneNumberInvalid;
            }

            if (phoneNumber.Length > PhoneNumberMaxLength)
            {
                return StaffErrors.PhoneNumberTooLong;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                return StaffErrors.PhoneNumberInvalid;
            }
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
