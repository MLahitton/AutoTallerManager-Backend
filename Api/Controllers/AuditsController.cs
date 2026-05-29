using Application.Features.Audits;
using Application.Features.Audits.Requests;
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.CreateAsync(request, cancellationToken);
        return FromResult(result, audit => CreatedAtAction(nameof(GetById), new { id = audit.AuditId }, audit));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, audit => Ok(audit));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _auditService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
