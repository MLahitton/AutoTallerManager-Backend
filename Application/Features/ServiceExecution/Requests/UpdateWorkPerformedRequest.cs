namespace Application.Features.ServiceExecution.Requests;

public class UpdateWorkPerformedRequest
{
    public string? WorkPerformed { get; set; }
    public decimal LaborCost { get; set; }
}
