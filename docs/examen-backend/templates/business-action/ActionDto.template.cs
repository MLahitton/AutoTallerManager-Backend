// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewFeature/Dtos/NewFeatureActionResultDto.cs
// Referencia: Application/Features/InventoryBusiness/Dtos/InventoryPurchaseCancellationResultDto.cs
//             Application/Features/PaymentBusiness/Dtos/RecordedPaymentDto.cs
//             Application/Features/InvoiceBusiness/Dtos/GeneratedInvoiceDto.cs

namespace Application.Features.NewFeature.Dtos;

// DTO de respuesta de la acción (no confundir con el DTO CRUD de la entidad).
public class NewFeatureActionResultDto
{
    public int NewEntityId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }

    // Incluir solo campos útiles para el cliente; no exponer entidades EF completas.
}
