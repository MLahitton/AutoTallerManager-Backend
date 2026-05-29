namespace Application.Features.Search.Dtos;

public class ServiceOrderSearchResultDto
{
    public int ServiceOrderId { get; set; }
    public int VehicleId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime EntryDate { get; set; }
    public string? GeneralDescription { get; set; }
}
