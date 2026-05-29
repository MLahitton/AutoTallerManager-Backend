namespace Application.Features.ServiceOrders.Requests;

public class CreateServiceOrderRequest
{
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
}
