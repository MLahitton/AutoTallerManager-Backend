namespace Domain.Entities;

public class VehicleOwnerHistory
{
    public int VehicleOwnerHistoryId { get; set; }
    public int VehicleId { get; set; }
    public int PersonId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public Person Person { get; set; } = null!;
}
