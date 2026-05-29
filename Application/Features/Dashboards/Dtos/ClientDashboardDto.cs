namespace Application.Features.Dashboards.Dtos;

public class ClientDashboardDto
{
    public int ActiveServiceOrders { get; set; }
    public int PendingApprovals { get; set; }
    public int PendingInvoices { get; set; }
    public int TotalVehicles { get; set; }
    public IReadOnlyList<int> RecentServiceOrderIds { get; set; } = Array.Empty<int>();
}
