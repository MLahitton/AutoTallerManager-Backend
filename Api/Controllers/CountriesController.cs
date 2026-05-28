using Application.Features.Countries;
using Application.Features.Countries.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/countries")]
public class CountriesController : BaseApiController
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _countryService.GetAllAsync(cancellationToken);
        return FromResult(result, countries => Ok(countries));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _countryService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, country => Ok(country));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCountryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _countryService.CreateAsync(request, cancellationToken);
        return FromResult(result, country => CreatedAtAction(nameof(GetById), new { id = country.CountryId }, country));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCountryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _countryService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, country => Ok(country));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _countryService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
