namespace Domain.Entities;

public class VehicleModel
{
    public int ModelId { get; set; }
    public int BrandId { get; set; }
    public string ModelName { get; set; } = string.Empty;

    public VehicleBrand Brand { get; set; } = null!;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
