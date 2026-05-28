namespace Application.Features.ServiceTypes.Dtos;

public class ServiceTypeDto
{
    public int ServiceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EstimatedDays { get; set; }
}
