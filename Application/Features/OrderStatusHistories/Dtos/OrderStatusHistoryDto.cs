namespace Application.Features.OrderStatusHistories.Dtos;

public class OrderStatusHistoryDto
{
    public int OrderStatusHistoryId { get; set; }
    public int ServiceOrderId { get; set; }
    public int? PreviousOrderStatusId { get; set; }
    public int NewOrderStatusId { get; set; }
    public int ChangedByUserId { get; set; }
    public string? Observation { get; set; }
    public DateTime ChangedAt { get; set; }
}
