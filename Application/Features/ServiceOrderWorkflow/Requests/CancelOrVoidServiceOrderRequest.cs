namespace Application.Features.ServiceOrderWorkflow.Requests;

public class CancelOrVoidServiceOrderRequest
{
    public string? Reason { get; set; }
    public string? Observation { get; set; }
}
