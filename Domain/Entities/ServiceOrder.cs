namespace Domain.Entities;

public class ServiceOrder
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public OrderStatus OrderStatus { get; set; } = null!;
    public VehicleEntryInventory? VehicleEntryInventory { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
    public ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
}
