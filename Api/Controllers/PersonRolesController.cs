using Application.Features.PersonRoles;
using Application.Features.PersonRoles.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/person-roles")]
[Authorize(Roles = "Admin")]
public class PersonRolesController : BaseApiController
{
    private readonly IPersonRoleService _personRoleService;

    public PersonRolesController(IPersonRoleService personRoleService)
    {
        _personRoleService = personRoleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _personRoleService.GetAllAsync(cancellationToken);
        return FromResult(result, personRoles => Ok(personRoles));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _personRoleService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, personRole => Ok(personRole));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personRoleService.CreateAsync(request, cancellationToken);
        return FromResult(result, personRole => CreatedAtAction(nameof(GetById), new { id = personRole.PersonRoleId }, personRole));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePersonRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personRoleService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, personRole => Ok(personRole));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _personRoleService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
