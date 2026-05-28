using Application.Features.Cities;
using Application.Features.Cities.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/cities")]
public class CitiesController : BaseApiController
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _cityService.GetAllAsync(cancellationToken);
        return FromResult(result, cities => Ok(cities));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _cityService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, city => Ok(city));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cityService.CreateAsync(request, cancellationToken);
        return FromResult(result, city => CreatedAtAction(nameof(GetById), new { id = city.CityId }, city));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cityService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, city => Ok(city));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _cityService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
