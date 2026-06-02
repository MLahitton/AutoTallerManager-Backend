namespace Domain.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }
    public int ModelId { get; set; }
    public int VehicleTypeId { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public bool IsActive { get; set; }

    public VehicleModel Model { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public ICollection<VehicleOwnerHistory> VehicleOwnerHistories { get; set; } = new List<VehicleOwnerHistory>();
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
}
