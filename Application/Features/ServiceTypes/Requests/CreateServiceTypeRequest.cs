namespace Application.Features.ServiceTypes.Requests;

public class CreateServiceTypeRequest
{
    public string? Name { get; set; }
    public int EstimatedDays { get; set; }
}
