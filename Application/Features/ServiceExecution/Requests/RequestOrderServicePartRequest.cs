namespace Application.Features.ServiceExecution.Requests;

public class RequestOrderServicePartRequest
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal? AppliedUnitPrice { get; set; }
}
