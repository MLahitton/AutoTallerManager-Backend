namespace Domain.Entities;

public class parte_nueva
{
    public int parte_nuevaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
