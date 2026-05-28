namespace Domain.Entities;

public class ServiceType
{
    public int ServiceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EstimatedDays { get; set; }

    public ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
}
