using Application.Features.MechanicAssignments;
using Application.Features.MechanicAssignments.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/mechanic-assignments")]
public class MechanicAssignmentsController : BaseApiController
{
    private readonly IMechanicAssignmentService _mechanicAssignmentService;

    public MechanicAssignmentsController(IMechanicAssignmentService mechanicAssignmentService)
    {
        _mechanicAssignmentService = mechanicAssignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mechanicAssignmentService.GetAllAsync(cancellationToken);
        return FromResult(result, mechanicAssignments => Ok(mechanicAssignments));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mechanicAssignmentService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, mechanicAssignment => Ok(mechanicAssignment));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMechanicAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mechanicAssignmentService.CreateAsync(request, cancellationToken);
        return FromResult(result, mechanicAssignment => CreatedAtAction(nameof(GetById), new { id = mechanicAssignment.MechanicAssignmentId }, mechanicAssignment));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMechanicAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mechanicAssignmentService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, mechanicAssignment => Ok(mechanicAssignment));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mechanicAssignmentService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
