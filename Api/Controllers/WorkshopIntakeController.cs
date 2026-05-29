using System.Security.Claims;
using Application.Features.WorkshopIntake;
using Application.Features.WorkshopIntake.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/workshop-intake")]
[Authorize(Roles = "Admin,Receptionist")]
public class WorkshopIntakeController : BaseApiController
{
    private readonly IWorkshopIntakeService _workshopIntakeService;

    public WorkshopIntakeController(IWorkshopIntakeService workshopIntakeService)
    {
        _workshopIntakeService = workshopIntakeService;
    }

    [HttpPost("create-service-order")]
    public async Task<IActionResult> CreateServiceOrder(
        [FromBody] CreateWorkshopIntakeRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue("userId");
        if (!int.TryParse(userIdClaim, out var changedByUserId) || changedByUserId <= 0)
        {
            return Unauthorized();
        }

        var result = await _workshopIntakeService.CreateServiceOrderAsync(
            changedByUserId,
            request,
            cancellationToken);

        return FromResult(result, workshopIntake =>
            CreatedAtAction(
                nameof(ServiceOrdersController.GetById),
                "ServiceOrders",
                new { id = workshopIntake.ServiceOrderId },
                workshopIntake));
    }
}
