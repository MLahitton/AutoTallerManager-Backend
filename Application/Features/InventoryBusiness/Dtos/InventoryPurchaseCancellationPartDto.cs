namespace Application.Features.InventoryBusiness.Dtos;

public class InventoryPurchaseCancellationPartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int PreviousStock { get; set; }
    public int ReversedQuantity { get; set; }
    public int NewStock { get; set; }
}
