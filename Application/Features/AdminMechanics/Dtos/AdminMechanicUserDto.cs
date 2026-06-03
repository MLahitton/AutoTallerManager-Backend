namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicUserDto
{
    public int UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
