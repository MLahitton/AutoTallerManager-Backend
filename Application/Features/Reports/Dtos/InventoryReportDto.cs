namespace Application.Features.Reports.Dtos;

public class InventoryReportDto
{
    public int TotalParts { get; set; }
    public int ActiveParts { get; set; }
    public int LowStockParts { get; set; }
    public int OutOfStockParts { get; set; }
    public decimal EstimatedInventoryValue { get; set; }
    public int PurchasesCount { get; set; }
    public decimal PurchasesAmount { get; set; }
}
