using Application.Features.Staff;
using Application.Features.Staff.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin")]
public class StaffController : BaseApiController
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _staffService.RegisterStaffAsync(request, cancellationToken);
        return FromResult(result, staffUser => Ok(staffUser));
    }
}
