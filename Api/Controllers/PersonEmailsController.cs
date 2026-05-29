using Application.Features.PersonEmails;
using Application.Features.PersonEmails.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/person-emails")]
[Authorize(Roles = "Admin,Receptionist")]
public class PersonEmailsController : BaseApiController
{
    private readonly IPersonEmailService _personEmailService;

    public PersonEmailsController(IPersonEmailService personEmailService)
    {
        _personEmailService = personEmailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _personEmailService.GetAllAsync(cancellationToken);
        return FromResult(result, personEmails => Ok(personEmails));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _personEmailService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, personEmail => Ok(personEmail));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personEmailService.CreateAsync(request, cancellationToken);
        return FromResult(result, personEmail => CreatedAtAction(nameof(GetById), new { id = personEmail.PersonEmailId }, personEmail));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePersonEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personEmailService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, personEmail => Ok(personEmail));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _personEmailService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
