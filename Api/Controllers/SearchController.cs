using Application.Features.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : BaseApiController
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("clients")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> SearchClients([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchClientsAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("vehicles")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> SearchVehicles([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchVehiclesAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("service-orders")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> SearchServiceOrders([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchServiceOrdersAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("invoices")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> SearchInvoices([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchInvoicesAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("parts")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> SearchParts([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchPartsAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("suppliers")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> SearchSuppliers([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchSuppliersAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }

    [HttpGet("mechanics")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> SearchMechanics([FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchMechanicsAsync(term, cancellationToken);
        return FromResult(result, items => Ok(items));
    }
}
