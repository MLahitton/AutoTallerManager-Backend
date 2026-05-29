using Application.Common.Results;

namespace Application.Features.Auth.Errors;

public static class AuthErrors
{
    public static readonly Error EmailRequired = new("Auth.EmailRequired", "Email is required.");
    public static readonly Error EmailInvalid = new("Auth.EmailInvalid", "Email format is invalid.");
    public static readonly Error PasswordRequired = new("Auth.PasswordRequired", "Password is required.");
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid credentials.");
    public static readonly Error UserInactiveInvalid = new("Auth.UserInactiveInvalid", "User is inactive.");
    public static readonly Error RefreshTokenRequired = new("Auth.RefreshTokenRequired", "Refresh token is required.");
    public static readonly Error RefreshTokenInvalid = new("Auth.RefreshTokenInvalid", "Refresh token is invalid.");
    public static readonly Error RefreshTokenExpired = new("Auth.RefreshTokenExpired", "Refresh token has expired.");
    public static readonly Error UserEmailNotFound = new("Auth.UserEmailNotFound", "User email could not be resolved.");
}
