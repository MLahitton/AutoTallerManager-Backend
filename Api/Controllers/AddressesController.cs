using Application.Features.Addresses;
using Application.Features.Addresses.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize(Roles = "Admin,Receptionist")]
public class AddressesController : BaseApiController
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _addressService.GetAllAsync(cancellationToken);
        return FromResult(result, addresses => Ok(addresses));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _addressService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, address => Ok(address));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.CreateAsync(request, cancellationToken);
        return FromResult(result, address => CreatedAtAction(nameof(GetById), new { id = address.AddressId }, address));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, address => Ok(address));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _addressService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
