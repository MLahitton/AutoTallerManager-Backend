namespace Domain.Entities;

public class OrderStatus
{
    public int OrderStatusId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
    public ICollection<OrderStatusHistory> PreviousOrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
    public ICollection<OrderStatusHistory> NewOrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
}
