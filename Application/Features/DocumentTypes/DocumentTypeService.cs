using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.DocumentTypes.Dtos;
using Application.Features.DocumentTypes.Errors;
using Application.Features.DocumentTypes.Requests;
using Domain.Entities;

namespace Application.Features.DocumentTypes;

public class DocumentTypeService : IDocumentTypeService
{
    private const int CodeMaxLength = 10;
    private const int NameMaxLength = 80;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<DocumentTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentTypes = await documentTypeRepository.GetAllAsync(cancellationToken);

        var documentTypeDtos = documentTypes
            .OrderBy(x => x.DocumentTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<DocumentTypeDto>>.Success(documentTypeDtos);
    }

    public async Task<Result<DocumentTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentType = await documentTypeRepository.GetByIdAsync(id, cancellationToken);

        if (documentType is null)
        {
            return Result<DocumentTypeDto>.Failure(DocumentTypeErrors.NotFound);
        }

        return Result<DocumentTypeDto>.Success(MapToDto(documentType));
    }

    public async Task<Result<DocumentTypeDto>> CreateAsync(
        CreateDocumentTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(request?.Code);
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(normalizedCode, normalizedName);
        if (validationError is not null)
        {
            return Result<DocumentTypeDto>.Failure(validationError);
        }

        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();

        var codeAlreadyExists = await documentTypeRepository.ExistsAsync(
            x => x.Code == normalizedCode,
            cancellationToken);
        if (codeAlreadyExists)
        {
            return Result<DocumentTypeDto>.Failure(DocumentTypeErrors.CodeAlreadyExists);
        }

        var nameAlreadyExists = await documentTypeRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);
        if (nameAlreadyExists)
        {
            return Result<DocumentTypeDto>.Failure(DocumentTypeErrors.NameAlreadyExists);
        }

        var documentType = new DocumentType
        {
            Code = normalizedCode,
            Name = normalizedName
        };

        await documentTypeRepository.AddAsync(documentType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DocumentTypeDto>.Success(MapToDto(documentType));
    }

    public async Task<Result<DocumentTypeDto>> UpdateAsync(
        int id,
        UpdateDocumentTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentType = await documentTypeRepository.GetByIdAsync(id, cancellationToken);

        if (documentType is null)
        {
            return Result<DocumentTypeDto>.Failure(DocumentTypeErrors.NotFound);
        }

        var normalizedCode = NormalizeCode(request?.Code);
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(normalizedCode, normalizedName);
        if (validationError is not null)
        {
            return Result<DocumentTypeDto>.Failure(validationError);
        }

        var codeAlreadyExists = await documentTypeRepository.ExistsAsync(
            x => x.Code == normalizedCode && x.DocumentTypeId != id,
            cancellationToken);
        if (codeAlreadyExists)
        {
            return Result<DocumentTypeDto>.Failure(DocumentTypeErrors.CodeAlreadyExists);
        }

        var nameAlreadyExists = await documentTypeRepository.ExistsAsync(
            x => x.Name == normalizedName && x.DocumentTypeId != id,
            cancellationToken);
        if (nameAlreadyExists)
        {
            return Result<DocumentTypeDto>.Failure(DocumentTypeErrors.NameAlreadyExists);
        }

        documentType.Code = normalizedCode;
        documentType.Name = normalizedName;

        documentTypeRepository.Update(documentType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DocumentTypeDto>.Success(MapToDto(documentType));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var documentTypeRepository = _unitOfWork.Repository<DocumentType>();
        var documentType = await documentTypeRepository.GetByIdAsync(id, cancellationToken);

        if (documentType is null)
        {
            return Result.Failure(DocumentTypeErrors.NotFound);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var isInUse = await personRepository.ExistsAsync(
            x => x.DocumentTypeId == id,
            cancellationToken);

        if (isInUse)
        {
            return Result.Failure(DocumentTypeErrors.InUse);
        }

        documentTypeRepository.Remove(documentType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static DocumentTypeDto MapToDto(DocumentType documentType)
    {
        return new DocumentTypeDto
        {
            DocumentTypeId = documentType.DocumentTypeId,
            Code = documentType.Code,
            Name = documentType.Name
        };
    }

    private static string NormalizeCode(string? code)
    {
        return (code ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? Validate(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return DocumentTypeErrors.CodeRequired;
        }

        if (code.Length > CodeMaxLength)
        {
            return DocumentTypeErrors.CodeTooLong;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return DocumentTypeErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return DocumentTypeErrors.NameTooLong;
        }

        return null;
    }
}
