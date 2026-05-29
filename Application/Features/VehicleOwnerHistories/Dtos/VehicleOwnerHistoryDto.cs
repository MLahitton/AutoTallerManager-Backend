namespace Application.Features.VehicleOwnerHistories.Dtos;

public class VehicleOwnerHistoryDto
{
    public int VehicleOwnerHistoryId { get; set; }
    public int VehicleId { get; set; }
    public int PersonId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
