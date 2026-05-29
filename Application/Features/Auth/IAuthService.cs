using Application.Common.Results;
using Application.Features.Auth.Dtos;
using Application.Features.Auth.Requests;

namespace Application.Features.Auth;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
