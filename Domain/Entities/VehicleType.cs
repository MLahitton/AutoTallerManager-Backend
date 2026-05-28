namespace Domain.Entities;

public class VehicleType
{
    public int VehicleTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
