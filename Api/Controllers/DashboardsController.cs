using System.Security.Claims;
using Application.Features.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class DashboardsController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardsController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("client/dashboard")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetClientDashboard(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetClientDashboardAsync(currentPersonId, cancellationToken);
        return FromResult(result, dashboard => Ok(dashboard));
    }

    [HttpGet("mechanic/dashboard")]
    [Authorize(Roles = "Mechanic")]
    public async Task<IActionResult> GetMechanicDashboard(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetMechanicDashboardAsync(currentPersonId, cancellationToken);
        return FromResult(result, dashboard => Ok(dashboard));
    }

    [HttpGet("receptionist/dashboard")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> GetReceptionistDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetReceptionistDashboardAsync(cancellationToken);
        return FromResult(result, dashboard => Ok(dashboard));
    }

    [HttpGet("admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAdminDashboardAsync(cancellationToken);
        return FromResult(result, dashboard => Ok(dashboard));
    }

    private bool TryGetCurrentPersonId(out int personId)
    {
        personId = 0;
        var claim = User.FindFirstValue("personId");
        return int.TryParse(claim, out personId) && personId > 0;
    }
}
