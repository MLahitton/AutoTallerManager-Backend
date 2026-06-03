using System.Security.Claims;
using Application.Features.Users;
using Application.Features.Users.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(cancellationToken);
        return FromResult(result, users => Ok(users));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, user => Ok(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _userService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, user => CreatedAtAction(nameof(GetById), new { id = user.UserId }, user));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _userService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, user => Ok(user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _userService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int currentUserId)
    {
        currentUserId = 0;
        var userIdClaim = User.FindFirstValue("userId");

        return int.TryParse(userIdClaim, out currentUserId) && currentUserId > 0;
    }
}
