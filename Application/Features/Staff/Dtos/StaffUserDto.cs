namespace Application.Features.Staff.Dtos;

public class StaffUserDto
{
    public int UserId { get; set; }
    public int PersonId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<int> SpecialtyIds { get; set; } = Array.Empty<int>();
}
