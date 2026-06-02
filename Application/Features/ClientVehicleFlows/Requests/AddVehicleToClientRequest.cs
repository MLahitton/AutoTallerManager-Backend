namespace Application.Features.ClientVehicleFlows.Requests;

public class AddVehicleToClientRequest
{
    public int ModelId { get; set; }
    public int VehicleTypeId { get; set; }
    public string? VIN { get; set; }
    public string? Plate { get; set; }
    public int Year { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
}
