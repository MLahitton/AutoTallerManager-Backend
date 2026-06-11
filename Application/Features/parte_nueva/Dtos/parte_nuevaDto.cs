namespace Application.Features.parte_nueva.Dtos;

public class parte_nuevaDto
{
    public int parte_nuevaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
