using Application.Features.AdminMechanics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/mechanics")]
[Authorize(Roles = "Admin")]
public class AdminMechanicsController : BaseApiController
{
    private readonly IAdminMechanicsService _adminMechanicsService;

    public AdminMechanicsController(IAdminMechanicsService adminMechanicsService)
    {
        _adminMechanicsService = adminMechanicsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _adminMechanicsService.GetAllAsync(cancellationToken);
        return FromResult(result, mechanics => Ok(mechanics));
    }

    [HttpGet("{personId:int}")]
    public async Task<IActionResult> GetByPersonId(int personId, CancellationToken cancellationToken)
    {
        var result = await _adminMechanicsService.GetByPersonIdAsync(personId, cancellationToken);
        return FromResult(result, mechanic => Ok(mechanic));
    }

    [HttpGet("{personId:int}/workload")]
    public async Task<IActionResult> GetWorkload(int personId, CancellationToken cancellationToken)
    {
        var result = await _adminMechanicsService.GetWorkloadAsync(personId, cancellationToken);
        return FromResult(result, workload => Ok(workload));
    }
}
