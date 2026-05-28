using Application.Features.PersonPhones;
using Application.Features.PersonPhones.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/person-phones")]
public class PersonPhonesController : BaseApiController
{
    private readonly IPersonPhoneService _personPhoneService;

    public PersonPhonesController(IPersonPhoneService personPhoneService)
    {
        _personPhoneService = personPhoneService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _personPhoneService.GetAllAsync(cancellationToken);
        return FromResult(result, personPhones => Ok(personPhones));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _personPhoneService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, personPhone => Ok(personPhone));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonPhoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personPhoneService.CreateAsync(request, cancellationToken);
        return FromResult(result, personPhone => CreatedAtAction(nameof(GetById), new { id = personPhone.PersonPhoneId }, personPhone));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePersonPhoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personPhoneService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, personPhone => Ok(personPhone));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _personPhoneService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
