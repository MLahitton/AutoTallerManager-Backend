using Application.Common.Security;
using Microsoft.Extensions.Options;

namespace Api.Security;

public class AuthTokenSettings : IAuthTokenSettings
{
    private readonly JwtOptions _jwtOptions;

    public AuthTokenSettings(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public int RefreshTokenExpirationDays => _jwtOptions.RefreshTokenExpirationDays;
}
