// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewFeature/Requests/ExecuteNewFeatureActionRequest.cs
// Referencia: Application/Features/InventoryBusiness/Requests/CancelInventoryPurchaseRequest.cs
//             Application/Features/InvoiceBusiness/Requests/GenerateInvoiceFromServiceOrderRequest.cs
//             Application/Features/PaymentBusiness/Requests/RecordPaymentRequest.cs
//
// Usar solo si la acción necesita body. Si solo usa el id de ruta, el request puede omitirse.

namespace Application.Features.NewFeature.Requests;

public class ExecuteNewFeatureActionRequest
{
    // Ejemplo: motivo de cancelación
    public string? Reason { get; set; }

    // Ejemplo: datos adicionales de la acción
    // public decimal Amount { get; set; }
    // public DateTime? ActionDate { get; set; }
}
