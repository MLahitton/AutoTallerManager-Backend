using System.Security.Cryptography;

namespace Application.Common.Security;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSize = 64;

    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenSize);
        return Convert.ToBase64String(bytes);
    }
}
