namespace Application.Features.Bootstrap;

public sealed class BootstrapAdminResultDto
{
    public bool Created { get; set; }
    public bool Skipped { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? PersonId { get; set; }
    public string? Email { get; set; }
}
