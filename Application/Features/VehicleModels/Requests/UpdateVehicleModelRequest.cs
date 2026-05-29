namespace Application.Features.VehicleModels.Requests;

public class UpdateVehicleModelRequest
{
    public int BrandId { get; set; }
    public string? ModelName { get; set; }
}
