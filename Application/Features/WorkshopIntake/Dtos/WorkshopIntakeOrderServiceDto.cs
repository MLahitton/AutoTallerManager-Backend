namespace Application.Features.WorkshopIntake.Dtos;

public class WorkshopIntakeOrderServiceDto
{
    public int OrderServiceId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? Description { get; set; }
    public decimal LaborCost { get; set; }
}
