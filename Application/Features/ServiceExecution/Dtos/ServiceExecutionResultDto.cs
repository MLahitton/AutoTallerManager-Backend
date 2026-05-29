namespace Application.Features.ServiceExecution.Dtos;

public class ServiceExecutionResultDto
{
    public int Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
}
