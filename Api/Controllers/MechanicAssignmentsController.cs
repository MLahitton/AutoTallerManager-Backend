using System.Security.Claims;
using Application.Features.MechanicAssignments;
using Application.Features.MechanicAssignments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/mechanic-assignments")]
[Authorize(Roles = "Admin,Receptionist")]
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
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _mechanicAssignmentService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, mechanicAssignment => CreatedAtAction(nameof(GetById), new { id = mechanicAssignment.MechanicAssignmentId }, mechanicAssignment));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMechanicAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _mechanicAssignmentService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, mechanicAssignment => Ok(mechanicAssignment));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _mechanicAssignmentService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
