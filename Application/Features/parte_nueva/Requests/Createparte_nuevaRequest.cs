namespace Application.Features.parte_nueva.Requests;

public class Createparte_nuevaRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
