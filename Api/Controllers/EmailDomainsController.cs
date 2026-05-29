using Application.Features.EmailDomains;
using Application.Features.EmailDomains.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/email-domains")]
[Authorize(Roles = "Admin")]
public class EmailDomainsController : BaseApiController
{
    private readonly IEmailDomainService _emailDomainService;

    public EmailDomainsController(IEmailDomainService emailDomainService)
    {
        _emailDomainService = emailDomainService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _emailDomainService.GetAllAsync(cancellationToken);
        return FromResult(result, emailDomains => Ok(emailDomains));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _emailDomainService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, emailDomain => Ok(emailDomain));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmailDomainRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _emailDomainService.CreateAsync(request, cancellationToken);
        return FromResult(result, emailDomain => CreatedAtAction(nameof(GetById), new { id = emailDomain.EmailDomainId }, emailDomain));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEmailDomainRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _emailDomainService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, emailDomain => Ok(emailDomain));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _emailDomainService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
