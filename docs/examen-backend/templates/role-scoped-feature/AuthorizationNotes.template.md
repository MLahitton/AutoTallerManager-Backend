# Notas: autorización por rol

## Atributo en controller

```csharp
[Authorize(Roles = "Admin,Receptionist")]
```

- Varios roles separados por coma = **cualquiera** de ellos puede acceder.
- Sin `[AllowAnonymous]`, el endpoint requiere JWT válido.

## Roles del sistema

| Rol | Alcance típico |
|-----|----------------|
| **Admin** | Todo: reportes, cancelaciones, auditoría, gestión de mecánicos |
| **Receptionist** | Operación diaria: vehículos, clientes, inventario, órdenes |
| **Mechanic** | Servicios asignados, ejecución, solicitud de partes |
| **Client** | Sus vehículos, órdenes, aprobaciones, facturas propias |

## Dos capas de seguridad

1. **ASP.NET Authorization** (`[Authorize(Roles = "...")]`) → ¿tiene el rol?
2. **Lógica en servicio** → ¿puede acceder a **este** registro?

Ejemplo: un Client tiene rol correcto pero no puede aprobar la orden de otro cliente → `AccessDeniedConflict` (409) en el servicio.

## Endpoints públicos

Solo auth y catálogos públicos usan `[AllowAnonymous]`:

- `POST /api/auth/login`
- `POST /api/auth/register-client`
- `GET /api/catalogs/public-registration`

Referencia: `Api/Controllers/AuthController.cs`, `CatalogsController.cs`

## Admin-only

```csharp
[Authorize(Roles = "Admin")]
```

Referencias: `ReportsController`, `AdminMechanicsController`, `AdminAuditQueriesController`, cancelación de compra en `InventoryBusinessController`.

## Mapeo HTTP para Forbidden

Si el servicio devuelve un error con código terminado en `Forbidden` → 403 (`BaseApiController`).

La mayoría de conflictos de ownership usan sufijo `Conflict` → 409.
