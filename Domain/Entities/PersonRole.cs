namespace Domain.Entities;

public class PersonRole
{
    public int PersonRoleId { get; set; }
    public int PersonId { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; }

    public Person Person { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
