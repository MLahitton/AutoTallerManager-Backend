using Application.Features.ClientVehicleFlows;
using Application.Features.ClientVehicleFlows.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/receptionist")]
[Authorize(Roles = "Admin,Receptionist")]
public class ReceptionistClientController : BaseApiController
{
    private readonly IClientVehicleFlowService _clientVehicleFlowService;

    public ReceptionistClientController(IClientVehicleFlowService clientVehicleFlowService)
    {
        _clientVehicleFlowService = clientVehicleFlowService;
    }

    [HttpPost("create-client-with-vehicle")]
    public async Task<IActionResult> CreateClientWithVehicle(
        [FromBody] CreateClientWithVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clientVehicleFlowService.CreateClientWithVehicleAsync(request, cancellationToken);
        return FromResult(result, created => Ok(created));
    }
}
