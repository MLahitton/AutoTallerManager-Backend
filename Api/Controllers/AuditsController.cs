using Application.Features.Audits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/audits")]
[Authorize(Roles = "Admin")]
public class AuditsController : BaseApiController
{
    private readonly IAuditService _auditService;

    public AuditsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _auditService.GetAllAsync(cancellationToken);
        return FromResult(result, audits => Ok(audits));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _auditService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, audit => Ok(audit));
    }
}
