namespace Application.Features.Users.Dtos;

public class UserDto
{
    public int UserId { get; set; }
    public int PersonId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
