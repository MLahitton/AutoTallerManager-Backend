using Application.Common.Results;
using Application.Features.DocumentTypes.Dtos;
using Application.Features.DocumentTypes.Requests;

namespace Application.Features.DocumentTypes;

public interface IDocumentTypeService
{
    Task<Result<IReadOnlyList<DocumentTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<DocumentTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<DocumentTypeDto>> CreateAsync(CreateDocumentTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result<DocumentTypeDto>> UpdateAsync(int id, UpdateDocumentTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
