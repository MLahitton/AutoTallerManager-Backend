namespace Application.Features.InventoryBusiness.Dtos;

public class InventorySummaryDto
{
    public int TotalParts { get; set; }
    public int ActiveParts { get; set; }
    public int LowStockParts { get; set; }
    public int OutOfStockParts { get; set; }
    public decimal EstimatedInventoryValue { get; set; }
}
