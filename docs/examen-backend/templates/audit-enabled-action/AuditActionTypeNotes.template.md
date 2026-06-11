# Notas: tipos de acción de auditoría

## Tabla `AuditActionTypes`

Catálogo seeded en base de datos. `AuditLogger` busca por **nombre** (case insensitive).

## Nombres usados en código del proyecto

| Nombre | Uso típico |
|--------|------------|
| `CREATE` | Alta de registro |
| `UPDATE` | Modificación |
| `DELETE` | Eliminación |
| `CANCEL` | Cancelación de compra, factura, etc. |

## Dónde se definen

- Entidad: `Domain/Entities/AuditActionType.cs`
- Seed: `Infrastructure/Persistence/Seeders/DatabaseSeeder.cs` (o configuración de seed en EF)

## Si el enunciado pide un tipo nuevo

1. Agregar fila en seed **o** migración de datos.
2. Usar el **mismo string** en `LogAsync`.
3. Si el tipo no existe, `AuditLogger` **silenciosamente no registra** (retorna sin error).

## Campos del registro Audit

| Campo | Origen |
|-------|--------|
| `UserId` | Parámetro `userId` de LogAsync |
| `AuditActionTypeId` | Resuelto por nombre |
| `AffectedEntity` | Parámetro `affectedEntity` |
| `AffectedRecordId` | Parámetro `affectedRecordId` |
| `Description` | Parámetro opcional |
| `CreatedAt` | `DateTime.UtcNow` en AuditLogger |

## API de consulta (solo Admin)

- `GET /api/audits` — CRUD lectura básica
- `GET /api/admin/audits/recent`
- `GET /api/admin/audits/by-entity`
- `GET /api/admin/audits/by-user/{userId}`

**No** crear endpoints POST para insertar auditorías manualmente.
