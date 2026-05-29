namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderWorkflowDto
{
    public int ServiceOrderId { get; set; }
    public int PreviousOrderStatusId { get; set; }
    public int NewOrderStatusId { get; set; }
    public int OrderStatusHistoryId { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
}
