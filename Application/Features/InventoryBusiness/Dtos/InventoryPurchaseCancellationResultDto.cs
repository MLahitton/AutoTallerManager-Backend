namespace Application.Features.InventoryBusiness.Dtos;

public class InventoryPurchaseCancellationResultDto
{
    public int PartPurchaseId { get; set; }
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Total { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public int? CancelledByUserId { get; set; }
    public IReadOnlyList<InventoryPurchaseCancellationPartDto> AffectedParts { get; set; } = Array.Empty<InventoryPurchaseCancellationPartDto>();
    public string Message { get; set; } = string.Empty;
}
