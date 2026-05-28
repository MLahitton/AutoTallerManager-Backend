namespace Application.Features.PersonRoles.Dtos;

public class PersonRoleDto
{
    public int PersonRoleId { get; set; }
    public int PersonId { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
}
