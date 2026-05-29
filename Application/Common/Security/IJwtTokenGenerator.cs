namespace Application.Common.Security;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(int userId, int personId, string email, IReadOnlyList<string> roles);
}
