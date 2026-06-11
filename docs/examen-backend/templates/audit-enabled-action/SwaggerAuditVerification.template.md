# Verificar auditoría en Swagger

## Requisitos

- Token de **Admin**
- Acción de negocio o CRUD ya ejecutada con auditoría habilitada

## 1. Login Admin

`POST /api/auth/login` con credenciales de administrador.

## 2. Ejecutar la acción auditada

Ejemplo: crear proveedor, cancelar compra, registrar pago.

`POST /api/suppliers` (Receptionist/Admin con userId en token)

## 3. Consultar auditoría reciente

`GET /api/admin/audits/recent`

Buscar entrada con:

- `affectedEntity` = nombre usado en `LogAsync` (ej. `Supplier`, `PartPurchase`)
- `affectedRecordId` = id del registro
- `userId` = usuario que ejecutó la acción

## 4. Consulta por entidad

`GET /api/admin/audits/by-entity?entity=Supplier&recordId=1`

Parámetros:

| Query | Valor ejemplo |
|-------|---------------|
| `entity` | `Supplier` |
| `recordId` | `1` |

## 5. Consulta por usuario

`GET /api/admin/audits/by-user/{userId}`

## Qué confirmar

- [ ] Existe registro después de la acción.
- [ ] `auditActionType` coincide (CREATE, UPDATE, CANCEL, ...).
- [ ] `description` no contiene contraseñas ni datos de tarjeta.
- [ ] `userId` corresponde al token usado, no a un id del body.

## Referencias de controllers

- `Api/Controllers/AdminAuditQueriesController.cs`
- `Api/Controllers/AuditsController.cs`

## Si no aparece auditoría

1. ¿El `actionTypeName` existe en `AuditActionTypes`?
2. ¿Llamaste el segundo `SaveChangesAsync` después de `LogAsync`?
3. ¿`userId` y `affectedRecordId` son > 0?
