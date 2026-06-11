// PLANTILLA DE ESTUDIO — NO COMPILAR
// Fragmento para insertar dentro de un método de servicio de negocio.
// Referencia: SupplierService.CreateAsync / UpdateAsync / DeleteAsync
//             InventoryBusinessService.CancelPurchaseAsync
//             InvoiceBusinessService.GenerateFromServiceOrderAsync
//             PaymentBusinessService.RecordPaymentAsync

// Requisitos previos en el servicio:
// - Inyectar IAuditLogger _auditLogger en el constructor
// - Tener currentUserId (int) del controller
// - Preferir ExecuteInTransactionAsync si hay varias escrituras

// Constantes típicas (copiar al inicio de la clase del servicio):
// private const string CreateAuditActionTypeName = "CREATE";
// private const string UpdateAuditActionTypeName = "UPDATE";
// private const string DeleteAuditActionTypeName = "DELETE";
// private const string CancelAuditActionTypeName = "CANCEL";

// --- Dentro de la transacción, DESPUÉS del primer SaveChangesAsync ---

await _auditLogger.LogAsync(
    currentUserId,
    "CANCEL",                    // Debe existir en tabla AuditActionTypes (seed)
    "PartPurchase",              // Nombre lógico de la entidad afectada
    purchase.PartPurchaseId,     // Id del registro afectado
    $"Purchase {purchase.PartPurchaseId} cancelled. Reason: {reason}.",
    cancellationToken);

// Segundo guardado para persistir el Audit agregado al contexto
await _unitOfWork.SaveChangesAsync(cancellationToken);

// --- Qué NO registrar ---
// - Contraseñas, tokens, números de tarjeta completos
// - Datos personales innecesarios

// --- Verificar auditoría después en Swagger ---
// GET /api/admin/audits/recent  (Admin)
// GET /api/admin/audits/by-entity?entity=PartPurchase&recordId=1
