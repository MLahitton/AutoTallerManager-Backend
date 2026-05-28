using Application.Features.AuditActionTypes;
using Application.Features.AuditActionTypes.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/audit-action-types")]
public class AuditActionTypesController : BaseApiController
{
    private readonly IAuditActionTypeService _auditActionTypeService;

    public AuditActionTypesController(IAuditActionTypeService auditActionTypeService)
    {
        _auditActionTypeService = auditActionTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _auditActionTypeService.GetAllAsync(cancellationToken);
        return FromResult(result, auditActionTypes => Ok(auditActionTypes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _auditActionTypeService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, auditActionType => Ok(auditActionType));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAuditActionTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auditActionTypeService.CreateAsync(request, cancellationToken);
        return FromResult(result, auditActionType => CreatedAtAction(nameof(GetById), new { id = auditActionType.AuditActionTypeId }, auditActionType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAuditActionTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auditActionTypeService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, auditActionType => Ok(auditActionType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _auditActionTypeService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
