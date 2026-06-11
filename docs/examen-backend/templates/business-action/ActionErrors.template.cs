// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewFeature/Errors/NewFeatureErrors.cs
// Referencia: Application/Features/InventoryBusiness/Errors/InventoryBusinessErrors.cs
//             Application/Features/InvoiceBusiness/Errors/InvoiceBusinessErrors.cs
//             Application/Features/PaymentBusiness/Errors/PaymentBusinessErrors.cs

using Application.Common.Results;

namespace Application.Features.NewFeature.Errors;

public static class NewFeatureErrors
{
    // Validación → 400
    public static readonly Error NewEntityIdInvalid = new("NewFeature.NewEntityIdInvalid", "NewEntityId must be greater than 0.");
    public static readonly Error ReasonRequired = new("NewFeature.ReasonRequired", "Reason is required.");
    public static readonly Error CurrentUserInvalid = new("NewFeature.CurrentUserInvalid", "Current user is invalid.");

    // No encontrado → 404
    public static readonly Error NewEntityNotFound = new("NewFeature.NewEntityNotFound", "New entity was not found.");

    // Reglas de negocio → 409
    public static readonly Error AlreadyProcessedConflict = new("NewFeature.AlreadyProcessedConflict", "Action was already processed.");
    public static readonly Error CannotProcessConflict = new("NewFeature.CannotProcessConflict", "Entity cannot be processed in its current state.");
}
