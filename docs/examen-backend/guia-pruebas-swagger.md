# Guía de pruebas en Swagger

## 1. Ejecutar el backend

Desde la raíz del repositorio `AutoTallerManager-Backend`:

```bash
dotnet run --project Api\Api.csproj
```

El perfil HTTP por defecto expone la API en:

```txt
http://localhost:5077
```

También existe HTTPS en `https://localhost:7072`, pero para pruebas locales suele ser más simple usar el puerto **5077**.

## 2. Abrir Swagger

Navega a:

```txt
http://localhost:5077/swagger
```

Swagger solo está habilitado en **Development** (`Api/Program.cs`).

## 3. Login y autorización

1. Expande `POST /api/auth/login`.
2. Usa un cuerpo JSON con email y contraseña, por ejemplo:

```json
{
  "email": "admin@example.com",
  "password": "TuPassword123*"
}
```

3. En la respuesta, copia el valor de `accessToken`.
4. Haz clic en **Authorize** (candado arriba a la derecha).
5. Pega **solo el token** (Swagger ya espera el esquema Bearer; no escribas la palabra `Bearer` si la UI lo indica así en `Program.cs`).

### Cuentas demo (si `SeedData:DemoAccountsEnabled` está activo)

Referencia en `Infrastructure/Persistence/Seeders/DemoAccountsSeeder.cs`:

| Rol | Email demo | Contraseña |
|-----|------------|------------|
| Receptionist | tmedina@gmail.com | Testing123* |
| Mechanic | nzabala1@gmail.com | Testing123* |
| Client | rolito1@gmail.com | Testing123* |

El admin puede venir de `BootstrapAdmin` en `appsettings.Development.json` o del seeder principal.

## 4. Probar por rol

| Rol | Qué probar | Ejemplo de controller |
|-----|------------|----------------------|
| Admin | Reportes, cancelaciones, auditoría | `ReportsController`, `InventoryBusinessController` (cancel) |
| Receptionist | CRUD operativo, compras | `VehiclesController`, `InventoryBusinessController` |
| Mechanic | Servicios asignados | `MechanicWorkflowController` |
| Client | Datos propios, aprobaciones | `ClientApprovalsController`, `ClientVehiclesController` |

Si usas un token de **Client** en un endpoint `[Authorize(Roles = "Admin")]`, espera **403 Forbidden** o fallo de autorización antes de llegar al servicio.

## 5. Probar CRUD

| Método | Qué validar |
|--------|-------------|
| **POST** | 201 Created, cuerpo con DTO, header `Location` si usa `CreatedAtAction` |
| **GET** (lista) | 200 OK, arreglo de DTOs |
| **GET** `{id}` | 200 OK con id existente; 404 si no existe |
| **PUT** `{id}` | 200 OK con DTO actualizado; 404 / 400 / 409 según reglas |
| **DELETE** `{id}` | 204 No Content; 404 si no existe; 409 si está en uso |

Ejemplo de referencia: `GET/POST/PUT/DELETE` en `api/vehicles` (`VehiclesController`).

## 6. Probar acción de negocio

Las acciones de negocio suelen ser **POST** o **PUT** en rutas descriptivas, no CRUD estándar.

Ejemplos reales:

| Endpoint | Acción |
|----------|--------|
| `POST /api/inventory/register-purchase` | Registrar compra |
| `POST /api/inventory/purchases/{purchaseId}/cancel` | Cancelar compra |
| `POST /api/invoices/...` | Generar factura (ver `InvoiceBusinessController`) |
| `POST /api/payments/...` | Registrar pago (`PaymentBusinessController`) |
| `POST /api/client/order-services/{id}/approve` | Aprobar servicio |

Pasos:

1. Prepara datos previos (proveedor, partes, orden de servicio, etc.).
2. Ejecuta la acción con el body requerido.
3. Verifica respuesta 200 con DTO de resultado.
4. Repite con datos inválidos para ver **400** o **409**.

## 7. Probar validaciones (errores intencionales)

| Escenario | Cómo provocarlo | HTTP esperado |
|-----------|-----------------|---------------|
| Campo requerido faltante | Omitir propiedad obligatoria en POST/PUT | 400 |
| Valor duplicado | Crear dos registros con mismo VIN/placa/nombre único | 409 |
| Id inexistente | Usar `{id}` = 999999 | 404 |
| Rol incorrecto | Token de Client en endpoint Admin | 401/403 |
| Regla de negocio | Cancelar compra ya cancelada | 409 |
| Sin token | No autorizar en endpoint protegido | 401 |

El cuerpo de error del proyecto tiene forma:

```json
{
  "code": "Vehicles.PlateAlreadyExists",
  "message": "Plate already exists for an active vehicle."
}
```

## 8. Respuestas HTTP esperadas

| Código | Cuándo |
|--------|--------|
| **200 OK** | Lectura o acción exitosa con cuerpo |
| **201 Created** | Recurso creado (CRUD POST) |
| **204 No Content** | Eliminación exitosa |
| **400 Bad Request** | Validación (`Required`, `Invalid`, `TooLong`, etc.) |
| **401 Unauthorized** | Sin token o claims inválidos |
| **403 Forbidden** | Token válido pero sin rol (`Forbidden` en código de error) |
| **404 Not Found** | Recurso no existe (`NotFound` en código) |
| **409 Conflict** | Duplicado o regla de negocio (`Conflict`, `AlreadyExists`, `InUse`) |

El mapeo está en `Api/Controllers/BaseApiController.cs`.

## 9. Checklist antes de dar por terminado el cambio

```txt
[ ] dotnet build passes.
[ ] Swagger opens.
[ ] Login works.
[ ] Correct role token used.
[ ] Success case tested.
[ ] Validation error tested.
[ ] NotFound tested if applicable.
[ ] Conflict tested if applicable.
[ ] Database updated correctly.
[ ] Audit created if required.
```
