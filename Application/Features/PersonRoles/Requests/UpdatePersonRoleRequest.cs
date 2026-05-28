namespace Application.Features.PersonRoles.Requests;

public class UpdatePersonRoleRequest
{
    public int PersonId { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
}
