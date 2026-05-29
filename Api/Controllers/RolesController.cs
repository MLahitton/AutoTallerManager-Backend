using Application.Features.Roles;
using Application.Features.Roles.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return FromResult(result, roles => Ok(roles));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, role => Ok(role));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(request, cancellationToken);
        return FromResult(result, role => CreatedAtAction(nameof(GetById), new { id = role.RoleId }, role));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, role => Ok(role));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
