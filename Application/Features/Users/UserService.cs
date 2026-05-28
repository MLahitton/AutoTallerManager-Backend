using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Common.Security;
using Application.Features.Users.Dtos;
using Application.Features.Users.Errors;
using Application.Features.Users.Requests;
using Domain.Entities;

namespace Application.Features.Users;

public class UserService : IUserService
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var users = await userRepository.GetAllAsync(cancellationToken);

        var userDtos = users
            .OrderBy(x => x.UserId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<UserDto>>.Success(userDtos);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.Failure(UserErrors.NotFound);
        }

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var personId = request?.PersonId ?? 0;
        var password = NormalizePassword(request?.Password);

        var idValidationError = ValidatePersonId(personId);
        if (idValidationError is not null)
        {
            return Result<UserDto>.Failure(idValidationError);
        }

        var passwordValidationError = ValidateCreatePassword(password);
        if (passwordValidationError is not null)
        {
            return Result<UserDto>.Failure(passwordValidationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<UserDto>.Failure(UserErrors.PersonNotFound);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var personAlreadyHasUser = await userRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (personAlreadyHasUser)
        {
            return Result<UserDto>.Failure(UserErrors.PersonAlreadyExists);
        }

        var user = new User
        {
            PersonId = personId,
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = request?.IsActive ?? false
        };

        await userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.Failure(UserErrors.NotFound);
        }

        var personId = request?.PersonId ?? 0;
        var idValidationError = ValidatePersonId(personId);
        if (idValidationError is not null)
        {
            return Result<UserDto>.Failure(idValidationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<UserDto>.Failure(UserErrors.PersonNotFound);
        }

        var personAlreadyHasUser = await userRepository.ExistsAsync(
            x => x.PersonId == personId && x.UserId != id,
            cancellationToken);

        if (personAlreadyHasUser)
        {
            return Result<UserDto>.Failure(UserErrors.PersonAlreadyExists);
        }

        var newPassword = NormalizeOptionalPassword(request?.NewPassword);
        if (newPassword is not null)
        {
            var passwordValidationError = ValidateUpdatePassword(newPassword);
            if (passwordValidationError is not null)
            {
                return Result<UserDto>.Failure(passwordValidationError);
            }

            user.PasswordHash = _passwordHasher.Hash(newPassword);
        }

        user.PersonId = personId;
        user.IsActive = request?.IsActive ?? false;

        userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var inUseByOrderStatusHistory = await orderStatusHistoryRepository.ExistsAsync(
            x => x.ChangedByUserId == id,
            cancellationToken);

        if (inUseByOrderStatusHistory)
        {
            return Result.Failure(UserErrors.InUse);
        }

        var auditRepository = _unitOfWork.Repository<Audit>();
        var inUseByAudit = await auditRepository.ExistsAsync(
            x => x.UserId == id,
            cancellationToken);

        if (inUseByAudit)
        {
            return Result.Failure(UserErrors.InUse);
        }

        userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            PersonId = user.PersonId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    private static string NormalizePassword(string? password)
    {
        return (password ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalPassword(string? password)
    {
        var normalized = (password ?? string.Empty).Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    private static Error? ValidatePersonId(int personId)
    {
        return personId <= 0 ? UserErrors.PersonIdInvalid : null;
    }

    private static Error? ValidateCreatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return UserErrors.PasswordRequired;
        }

        return ValidatePasswordLength(password);
    }

    private static Error? ValidateUpdatePassword(string password)
    {
        return ValidatePasswordLength(password);
    }

    private static Error? ValidatePasswordLength(string password)
    {
        if (password.Length < MinPasswordLength)
        {
            return UserErrors.PasswordTooShort;
        }

        if (password.Length > MaxPasswordLength)
        {
            return UserErrors.PasswordTooLong;
        }

        return null;
    }
}
