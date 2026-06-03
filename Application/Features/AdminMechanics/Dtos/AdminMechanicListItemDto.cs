namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicListItemDto
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public bool? IsUserActive { get; set; }
    public bool RoleActive { get; set; }
    public IReadOnlyList<AdminMechanicSpecialtyDto> Specialties { get; set; } = Array.Empty<AdminMechanicSpecialtyDto>();
    public int AssignedServicesCount { get; set; }
    public int ActiveOrdersCount { get; set; }
}
