namespace Application.Features.VehicleModels.Requests;

public class CreateVehicleModelRequest
{
    public int BrandId { get; set; }
    public string? ModelName { get; set; }
}
