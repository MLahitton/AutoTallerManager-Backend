namespace Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public ICollection<PersonRole> PersonRoles { get; set; } = new List<PersonRole>();
}
