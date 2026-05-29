using Application.Features.Persons;
using Application.Features.Persons.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/persons")]
[Authorize(Roles = "Admin,Receptionist")]
public class PersonsController : BaseApiController
{
    private readonly IPersonService _personService;

    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _personService.GetAllAsync(cancellationToken);
        return FromResult(result, persons => Ok(persons));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _personService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, person => Ok(person));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personService.CreateAsync(request, cancellationToken);
        return FromResult(result, person => CreatedAtAction(nameof(GetById), new { id = person.PersonId }, person));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, person => Ok(person));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _personService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
