namespace Application.Features.ServiceOrders.Requests;

public class UpdateServiceOrderRequest
{
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? GeneralDescription { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
}
