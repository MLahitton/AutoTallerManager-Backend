using Application.Features.DocumentTypes;
using Application.Features.DocumentTypes.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/document-types")]
public class DocumentTypesController : BaseApiController
{
    private readonly IDocumentTypeService _documentTypeService;

    public DocumentTypesController(IDocumentTypeService documentTypeService)
    {
        _documentTypeService = documentTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _documentTypeService.GetAllAsync(cancellationToken);
        return FromResult(result, documentTypes => Ok(documentTypes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _documentTypeService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, documentType => Ok(documentType));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _documentTypeService.CreateAsync(request, cancellationToken);
        return FromResult(
            result,
            documentType => CreatedAtAction(nameof(GetById), new { id = documentType.DocumentTypeId }, documentType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _documentTypeService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, documentType => Ok(documentType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _documentTypeService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
