namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicDetailDto
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? DocumentTypeName { get; set; }
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsUserActive { get; set; }
    public bool RoleActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<AdminMechanicSpecialtyDto> Specialties { get; set; } = Array.Empty<AdminMechanicSpecialtyDto>();
    public IReadOnlyList<AdminMechanicRoleDto> Roles { get; set; } = Array.Empty<AdminMechanicRoleDto>();
    public AdminMechanicUserDto? User { get; set; }
    public int AssignedServicesCount { get; set; }
    public int ActiveOrdersCount { get; set; }
    public IReadOnlyList<AdminMechanicWorkloadServiceDto> AssignedServices { get; set; } = Array.Empty<AdminMechanicWorkloadServiceDto>();
    public IReadOnlyList<AdminMechanicActiveOrderDto> ActiveOrders { get; set; } = Array.Empty<AdminMechanicActiveOrderDto>();
}
