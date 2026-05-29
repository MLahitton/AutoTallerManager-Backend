namespace Application.Features.Vehicles.Requests;

public class UpdateVehicleRequest
{
    public int ModelId { get; set; }
    public int VehicleTypeId { get; set; }
    public string? VIN { get; set; }
    public int Year { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public bool IsActive { get; set; }
}
