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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IAuthTokenSettings _authTokenSettings;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAuthTokenSettings authTokenSettings)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _authTokenSettings = authTokenSettings;
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
