using Application.Features.Catalogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/catalogs")]
[Authorize(Roles = "Admin,Receptionist,Mechanic")]
public class CatalogsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public CatalogsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [AllowAnonymous]
    [HttpGet("public-registration")]
    public async Task<IActionResult> GetPublicRegistrationCatalogs(CancellationToken cancellationToken)
    {
        var result = await _catalogService.GetPublicRegistrationCatalogsAsync(cancellationToken);
        return FromResult(result, catalogs => Ok(catalogs));
    }

    [HttpGet("workshop")]
    public async Task<IActionResult> GetWorkshopCatalogs(CancellationToken cancellationToken)
    {
        var result = await _catalogService.GetWorkshopCatalogsAsync(cancellationToken);
        return FromResult(result, catalogs => Ok(catalogs));
    }
}
