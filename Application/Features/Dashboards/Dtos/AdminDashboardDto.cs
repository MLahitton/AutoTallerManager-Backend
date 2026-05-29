namespace Application.Features.Dashboards.Dtos;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalClients { get; set; }
    public int TotalMechanics { get; set; }
    public int ActiveServiceOrders { get; set; }
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int LowStockParts { get; set; }
    public decimal TotalInvoicedAmount { get; set; }
    public decimal TotalCompletedPaymentsAmount { get; set; }
    public decimal PendingPaymentAmount { get; set; }
}
