namespace Application.Features.Auth.Dtos;

public class AuthUserDto
{
    public int UserId { get; set; }
    public int PersonId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}
