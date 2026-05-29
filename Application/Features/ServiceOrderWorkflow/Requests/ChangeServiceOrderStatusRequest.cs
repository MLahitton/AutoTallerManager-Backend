namespace Application.Features.ServiceOrderWorkflow.Requests;

public class ChangeServiceOrderStatusRequest
{
    public int NewOrderStatusId { get; set; }
    public string? Observation { get; set; }
}
