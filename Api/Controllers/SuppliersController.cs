using System.Security.Claims;
using Application.Features.Suppliers;
using Application.Features.Suppliers.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize(Roles = "Admin,Receptionist")]
public class SuppliersController : BaseApiController
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _supplierService.GetAllAsync(cancellationToken);
        return FromResult(result, suppliers => Ok(suppliers));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _supplierService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, supplier => Ok(supplier));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _supplierService.CreateAsync(request, currentUserId, cancellationToken);
        return FromResult(result, supplier => CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId }, supplier));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _supplierService.UpdateAsync(id, request, currentUserId, cancellationToken);
        return FromResult(result, supplier => Ok(supplier));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _supplierService.DeleteAsync(id, currentUserId, cancellationToken);
        return FromResult(result, () => NoContent());
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;

        var userIdClaim = User.FindFirstValue("userId");
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
