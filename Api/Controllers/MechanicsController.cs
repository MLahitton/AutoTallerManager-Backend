using Application.Features.Staff;
using Application.Features.Staff.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/mechanics")]
public class MechanicsController : BaseApiController
{
    private readonly IStaffService _staffService;

    public MechanicsController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet("{personId:int}/specialties")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetSpecialties(int personId, CancellationToken cancellationToken)
    {
        var result = await _staffService.GetMechanicSpecialtiesAsync(personId, cancellationToken);
        return FromResult(result, specialties => Ok(specialties));
    }

    [HttpPut("{personId:int}/specialties")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReplaceSpecialties(
        int personId,
        [FromBody] ReplaceMechanicSpecialtiesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _staffService.ReplaceMechanicSpecialtiesAsync(personId, request, cancellationToken);
        return FromResult(result, specialties => Ok(specialties));
    }
}
