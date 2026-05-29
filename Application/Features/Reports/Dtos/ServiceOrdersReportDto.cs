namespace Application.Features.Reports.Dtos;

public class ServiceOrdersReportDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int VoidedOrders { get; set; }
}
