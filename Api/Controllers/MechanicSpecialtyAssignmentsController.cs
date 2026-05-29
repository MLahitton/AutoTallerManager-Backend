using Application.Features.MechanicSpecialtyAssignments;
using Application.Features.MechanicSpecialtyAssignments.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/mechanic-specialty-assignments")]
[Authorize(Roles = "Admin,Receptionist")]
public class MechanicSpecialtyAssignmentsController : BaseApiController
{
    private readonly IMechanicSpecialtyAssignmentService _mechanicSpecialtyAssignmentService;

    public MechanicSpecialtyAssignmentsController(IMechanicSpecialtyAssignmentService mechanicSpecialtyAssignmentService)
    {
        _mechanicSpecialtyAssignmentService = mechanicSpecialtyAssignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyAssignmentService.GetAllAsync(cancellationToken);
        return FromResult(result, assignments => Ok(assignments));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyAssignmentService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, assignment => Ok(assignment));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMechanicSpecialtyAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyAssignmentService.CreateAsync(request, cancellationToken);
        return FromResult(result, assignment => CreatedAtAction(nameof(GetById), new { id = assignment.AssignmentId }, assignment));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMechanicSpecialtyAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyAssignmentService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, assignment => Ok(assignment));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mechanicSpecialtyAssignmentService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
