namespace Domain.Entities;

public class VehicleBrand
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;

    public ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();
}
