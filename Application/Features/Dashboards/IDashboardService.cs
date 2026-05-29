using Application.Common.Results;
using Application.Features.Dashboards.Dtos;

namespace Application.Features.Dashboards;

public interface IDashboardService
{
    Task<Result<ClientDashboardDto>> GetClientDashboardAsync(int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<MechanicDashboardDto>> GetMechanicDashboardAsync(int currentPersonId, CancellationToken cancellationToken = default);
    Task<Result<ReceptionistDashboardDto>> GetReceptionistDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
}
