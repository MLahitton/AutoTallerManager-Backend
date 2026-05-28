namespace Application.Features.Users.Requests;

public class UpdateUserRequest
{
    public int PersonId { get; set; }
    public string? NewPassword { get; set; }
    public bool IsActive { get; set; }
}
