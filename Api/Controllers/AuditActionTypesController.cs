using Application.Features.AuditActionTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/audit-action-types")]
[Authorize(Roles = "Admin")]
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
}
