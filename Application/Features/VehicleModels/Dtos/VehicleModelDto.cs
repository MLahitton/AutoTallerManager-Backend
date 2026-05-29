namespace Application.Features.VehicleModels.Dtos;

public class VehicleModelDto
{
    public int ModelId { get; set; }
    public int BrandId { get; set; }
    public string ModelName { get; set; } = string.Empty;
}
