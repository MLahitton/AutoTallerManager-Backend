using System.Security.Claims;
using Application.Features.ClientVehicleFlows;
using Application.Features.ClientVehicleFlows.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class ClientVehiclesController : BaseApiController
{
    private readonly IClientVehicleFlowService _clientVehicleFlowService;

    public ClientVehiclesController(IClientVehicleFlowService clientVehicleFlowService)
    {
        _clientVehicleFlowService = clientVehicleFlowService;
    }

    [HttpPost("clients/{personId:int}/vehicles")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> AddVehicleToClient(
        int personId,
        [FromBody] AddVehicleToClientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clientVehicleFlowService.AddVehicleToClientAsync(personId, request, cancellationToken);
        return FromResult(result, vehicle => Ok(vehicle));
    }

    [HttpPost("vehicles/{vehicleId:int}/transfer-ownership")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> TransferVehicleOwnership(
        int vehicleId,
        [FromBody] TransferVehicleOwnershipRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clientVehicleFlowService.TransferVehicleOwnershipAsync(vehicleId, request, cancellationToken);
        return FromResult(result, vehicle => Ok(vehicle));
    }

    [HttpGet("clients/{personId:int}/vehicles")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetClientVehicles(int personId, CancellationToken cancellationToken)
    {
        var result = await _clientVehicleFlowService.GetClientVehiclesAsync(personId, cancellationToken);
        return FromResult(result, vehicles => Ok(vehicles));
    }

    [HttpGet("clients/{personId:int}/service-orders")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetClientServiceOrders(int personId, CancellationToken cancellationToken)
    {
        var result = await _clientVehicleFlowService.GetClientServiceOrdersAsync(personId, cancellationToken);
        return FromResult(result, serviceOrders => Ok(serviceOrders));
    }

    [HttpGet("client/my-vehicles")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyVehicles(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientVehicleFlowService.GetClientVehiclesAsync(currentPersonId, cancellationToken);
        return FromResult(result, vehicles => Ok(vehicles));
    }

    [HttpGet("client/my-service-orders")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyServiceOrders(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientVehicleFlowService.GetClientServiceOrdersAsync(currentPersonId, cancellationToken);
        return FromResult(result, serviceOrders => Ok(serviceOrders));
    }

    [HttpGet("client/my-invoices")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyInvoices(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentPersonId(out var currentPersonId))
        {
            return Unauthorized();
        }

        var result = await _clientVehicleFlowService.GetClientInvoicesAsync(currentPersonId, cancellationToken);
        return FromResult(result, invoices => Ok(invoices));
    }

    private bool TryGetCurrentPersonId(out int personId)
    {
        personId = 0;

        var personIdClaim = User.FindFirstValue("personId");
        return int.TryParse(personIdClaim, out personId) && personId > 0;
    }
}
