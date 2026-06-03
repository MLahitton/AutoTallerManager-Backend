namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicRoleDto
{
    public int PersonRoleId { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
