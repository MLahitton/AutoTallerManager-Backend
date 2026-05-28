using Application.Common.Results;

namespace Application.Features.DocumentTypes.Errors;

public static class DocumentTypeErrors
{
    public static readonly Error NotFound = new("DocumentTypes.NotFound", "Document type was not found.");
    public static readonly Error CodeRequired = new("DocumentTypes.CodeRequired", "Document type code is required.");
    public static readonly Error CodeTooLong = new("DocumentTypes.CodeTooLong", "Document type code cannot exceed 10 characters.");
    public static readonly Error NameRequired = new("DocumentTypes.NameRequired", "Document type name is required.");
    public static readonly Error NameTooLong = new("DocumentTypes.NameTooLong", "Document type name cannot exceed 80 characters.");
    public static readonly Error CodeAlreadyExists = new("DocumentTypes.CodeAlreadyExists", "Document type code already exists.");
    public static readonly Error NameAlreadyExists = new("DocumentTypes.NameAlreadyExists", "Document type name already exists.");
    public static readonly Error InUse = new("DocumentTypes.InUse", "Document type is assigned to one or more persons.");
}
