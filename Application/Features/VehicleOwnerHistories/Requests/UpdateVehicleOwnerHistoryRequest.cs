namespace Application.Features.VehicleOwnerHistories.Requests;

public class UpdateVehicleOwnerHistoryRequest
{
    public int VehicleId { get; set; }
    public int PersonId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
