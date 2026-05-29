namespace Application.Features.InventoryBusiness.Dtos;

public class InventoryAdjustmentResultDto
{
    public int PartId { get; set; }
    public int PreviousStock { get; set; }
    public int AdjustmentQuantity { get; set; }
    public int NewStock { get; set; }
    public string? Reason { get; set; }
}
