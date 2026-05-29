namespace Application.Features.ClientVehicleFlows.Requests;

public class TransferVehicleOwnershipRequest
{
    public int NewOwnerPersonId { get; set; }
    public DateTime? TransferDate { get; set; }
}
