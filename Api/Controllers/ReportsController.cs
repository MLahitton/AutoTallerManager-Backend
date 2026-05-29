using Application.Features.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetSalesReportAsync(from, to, cancellationToken);
        return FromResult(result, report => Ok(report));
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetInventoryReportAsync(from, to, cancellationToken);
        return FromResult(result, report => Ok(report));
    }

    [HttpGet("mechanics")]
    public async Task<IActionResult> GetMechanicsReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetMechanicsReportAsync(from, to, cancellationToken);
        return FromResult(result, report => Ok(report));
    }

    [HttpGet("service-orders")]
    public async Task<IActionResult> GetServiceOrdersReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetServiceOrdersReportAsync(from, to, cancellationToken);
        return FromResult(result, report => Ok(report));
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentsReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetPaymentsReportAsync(from, to, cancellationToken);
        return FromResult(result, report => Ok(report));
    }
}
