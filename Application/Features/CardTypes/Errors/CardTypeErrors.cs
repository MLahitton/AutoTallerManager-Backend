using Application.Common.Results;

namespace Application.Features.CardTypes.Errors;

public static class CardTypeErrors
{
    public static readonly Error NotFound = new("CardTypes.NotFound", "Card type was not found.");
    public static readonly Error NameRequired = new("CardTypes.NameRequired", "Card type name is required.");
    public static readonly Error NameTooLong = new("CardTypes.NameTooLong", "Card type name cannot exceed 50 characters.");
    public static readonly Error NameAlreadyExists = new("CardTypes.NameAlreadyExists", "Card type name already exists.");
    public static readonly Error InUse = new("CardTypes.InUse", "Card type is assigned to one or more payment cards.");
}
