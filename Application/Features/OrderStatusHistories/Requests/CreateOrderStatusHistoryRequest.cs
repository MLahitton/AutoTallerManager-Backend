namespace Application.Features.OrderStatusHistories.Requests;

public class CreateOrderStatusHistoryRequest
{
    public int ServiceOrderId { get; set; }
    public int? PreviousOrderStatusId { get; set; }
    public int NewOrderStatusId { get; set; }
    public int ChangedByUserId { get; set; }
    public string? Observation { get; set; }
}
