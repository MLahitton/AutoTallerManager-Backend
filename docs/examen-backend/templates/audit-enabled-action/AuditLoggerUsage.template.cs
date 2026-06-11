// PLANTILLA DE ESTUDIO — NO COMPILAR
// Uso de IAuditLogger en servicios de aplicación
// Referencia: Application/Common/Auditing/IAuditLogger.cs
//             Application/Common/Auditing/AuditLogger.cs
//             Application/Features/Suppliers/SupplierService.cs

// ========== 1. Inyectar en el constructor del servicio ==========

// private readonly IAuditLogger _auditLogger;
//
// public MyService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
// {
//     _unitOfWork = unitOfWork;
//     _auditLogger = auditLogger;
// }

// ========== 2. Llamar LogAsync después de mutar datos ==========

// await _auditLogger.LogAsync(
//     currentUserId,           // int — del claim userId, NO del body
//     "CREATE",                // string — debe existir en AuditActionTypes (seed)
//     "Supplier",              // string — nombre lógico de entidad
//     supplier.SupplierId,     // int — id del registro afectado
//     "Supplier created.",     // string? — descripción breve, sin datos sensibles
//     cancellationToken);

// ========== 3. Persistencia ==========
// AuditLogger solo hace AddAsync al repositorio Audit.
// El servicio DEBE llamar SaveChangesAsync después.
//
// Patrón con transacción (SupplierService, InventoryBusinessService):
//   await _unitOfWork.SaveChangesAsync(ct);      // guarda negocio
//   await _auditLogger.LogAsync(..., ct);        // agrega audit al contexto
//   await _unitOfWork.SaveChangesAsync(ct);      // guarda audit

// ========== 4. Tipos de acción comunes en el proyecto ==========
// CREATE, UPDATE, DELETE, CANCEL
// Definidos en seed de AuditActionTypes — usar el nombre exacto (case insensitive).

// ========== 5. Verificación (solo lectura) ==========
// GET /api/admin/audits/recent
// GET /api/admin/audits/by-entity?entity=Supplier&recordId=1
