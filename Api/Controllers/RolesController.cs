using Application.Common.Results;
using Application.Features.Roles;
using Application.Features.Roles.Errors;
using Application.Features.Roles.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
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
        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.RoleId }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return MapError(result.Error);
        }

        return NoContent();
    }

    private IActionResult MapError(Error error)
    {
        return error.Code switch
        {
            var code when code == RoleErrors.NotFound.Code => NotFound(ToErrorResponse(error)),
            var code when code == RoleErrors.NameRequired.Code => BadRequest(ToErrorResponse(error)),
            var code when code == RoleErrors.NameTooLong.Code => BadRequest(ToErrorResponse(error)),
            var code when code == RoleErrors.NameAlreadyExists.Code => Conflict(ToErrorResponse(error)),
            var code when code == RoleErrors.RoleInUse.Code => Conflict(ToErrorResponse(error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ToErrorResponse(error))
        };
    }

    private static object ToErrorResponse(Error error)
    {
        return new
        {
            error.Code,
            error.Message
        };
    }
}
