namespace Application.Features.Search.Dtos;

public class VehicleSearchResultDto
{
    public int VehicleId { get; set; }
    public string VIN { get; set; } = string.Empty;
    public int ModelId { get; set; }
    public int VehicleTypeId { get; set; }
    public int Year { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
}
