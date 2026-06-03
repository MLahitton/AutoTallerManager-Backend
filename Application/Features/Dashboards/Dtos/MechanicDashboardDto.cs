namespace Application.Features.Dashboards.Dtos;

public class MechanicDashboardDto
{
    public int AssignedServices { get; set; }
    public int ActiveOrders { get; set; }
    public int PendingWorkReports { get; set; }
    public int RequestedPartsPendingApproval { get; set; }
    public IReadOnlyList<int> ActiveServiceOrderIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<MechanicDashboardActiveOrderDto> ActiveOrdersPreview { get; set; } = Array.Empty<MechanicDashboardActiveOrderDto>();
}
