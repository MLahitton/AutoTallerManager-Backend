namespace Application.Features.WorkshopIntake.Requests;

public class CreateWorkshopIntakeOrderServiceRequest
{
    public int ServiceTypeId { get; set; }
    public string? Description { get; set; }
    public decimal LaborCost { get; set; }
}
