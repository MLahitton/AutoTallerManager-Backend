namespace Application.Features.Dashboards.Dtos;

public class ReceptionistDashboardDto
{
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrdersToday { get; set; }
    public int VehiclesCurrentlyInWorkshop { get; set; }
    public int PendingInvoices { get; set; }
    public int LowStockParts { get; set; }
}
