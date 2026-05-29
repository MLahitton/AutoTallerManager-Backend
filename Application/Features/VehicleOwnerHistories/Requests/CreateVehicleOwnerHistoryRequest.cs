namespace Application.Features.VehicleOwnerHistories.Requests;

public class CreateVehicleOwnerHistoryRequest
{
    public int VehicleId { get; set; }
    public int PersonId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
