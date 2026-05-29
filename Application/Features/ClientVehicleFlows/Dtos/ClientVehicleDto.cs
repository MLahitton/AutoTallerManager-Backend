namespace Application.Features.ClientVehicleFlows.Dtos;

public class ClientVehicleDto
{
    public int VehicleId { get; set; }
    public int ModelId { get; set; }
    public int VehicleTypeId { get; set; }
    public string VIN { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public bool IsActive { get; set; }
    public DateTime OwnershipStartDate { get; set; }
    public DateTime? OwnershipEndDate { get; set; }
}
