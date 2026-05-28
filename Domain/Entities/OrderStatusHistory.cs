namespace Domain.Entities;

public class OrderStatusHistory
{
    public int OrderStatusHistoryId { get; set; }
    public int ServiceOrderId { get; set; }
    public int? PreviousOrderStatusId { get; set; }
    public int NewOrderStatusId { get; set; }
    public int ChangedByUserId { get; set; }
    public string? Observation { get; set; }
    public DateTime ChangedAt { get; set; }

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public OrderStatus? PreviousOrderStatus { get; set; }
    public OrderStatus NewOrderStatus { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
