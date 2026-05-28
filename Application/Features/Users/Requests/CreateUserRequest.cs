namespace Application.Features.Users.Requests;

public class CreateUserRequest
{
    public int PersonId { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
}
