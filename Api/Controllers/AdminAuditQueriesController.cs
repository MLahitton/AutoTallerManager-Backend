using Application.Features.AuditQueries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/audits")]
[Authorize(Roles = "Admin")]
public class AdminAuditQueriesController : BaseApiController
{
    private readonly IAuditQueryService _auditQueryService;

    public AdminAuditQueriesController(IAuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
    {
        var result = await _auditQueryService.GetRecentAsync(cancellationToken);
        return FromResult(result, audits => Ok(audits));
    }

    [HttpGet("by-entity")]
    public async Task<IActionResult> GetByEntity([FromQuery] string? entity, [FromQuery] int recordId, CancellationToken cancellationToken)
    {
        var result = await _auditQueryService.GetByEntityAsync(entity, recordId, cancellationToken);
        return FromResult(result, audits => Ok(audits));
    }

    [HttpGet("by-user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId, CancellationToken cancellationToken)
    {
        var result = await _auditQueryService.GetByUserAsync(userId, cancellationToken);
        return FromResult(result, audits => Ok(audits));
    }
}
