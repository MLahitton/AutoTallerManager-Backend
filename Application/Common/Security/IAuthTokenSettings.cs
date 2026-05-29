namespace Application.Common.Security;

public interface IAuthTokenSettings
{
    int RefreshTokenExpirationDays { get; }
}
