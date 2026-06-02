namespace Application.Features.ClientVehicleFlows.Dtos;

public class ClientWithVehicleDto
{
    public int PersonId { get; set; }
    public int VehicleId { get; set; }
    public int VehicleOwnerHistoryId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhoneNumber { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
}
