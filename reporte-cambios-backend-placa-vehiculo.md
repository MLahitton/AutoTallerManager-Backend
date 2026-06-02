# Reporte de cambios backend: soporte de placa vehicular

Fecha de reporte: 2026-06-02

Proyecto: AutoTallerManager-Backend

Objetivo original: agregar soporte completo para la placa del vehículo en el backend, sin cambiar arquitectura, rutas, nombres de proyectos, controladores base, patrón `Result/Error`, ni hacer commit.

## Resumen ejecutivo

Se agregó el campo `Plate` al modelo de vehículo y se propagó por los flujos principales de creación, actualización, consulta, búsqueda y detalle de orden de servicio.

El backend ahora valida placa requerida, normalizada en mayúsculas, longitud entre 5 y 10 caracteres, formato alfanumérico con guiones opcionales y duplicidad contra vehículos activos.

La base de datos ya fue actualizada correctamente. La tabla `Vehicles` tiene la columna `Plate` como `varchar(10) NOT NULL` y el índice único `IX_Vehicles_Plate`.

Importante: durante el proceso se aplicó una migración vacía llamada `20260602164728_AddVehiclePlate_Fix`. Esa migración no hizo cambios de esquema, pero quedó registrada en `__EFMigrationsHistory`; no debe eliminarse si esa base ya la tiene aplicada. La migración real que aplicó la columna y el índice es `20260602170500_AddVehiclePlateColumn`.

## Estado final validado

Build ejecutado:

```powershell
dotnet build .\AutoTallerManager.slnx
```

Resultado:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Migraciones aplicadas según EF Core:

```text
20260528114245_InitialCreate
20260602164728_AddVehiclePlate_Fix
20260602170500_AddVehiclePlateColumn
```

Validación visual en MySQL Workbench:

```sql
SHOW COLUMNS FROM Vehicles LIKE 'Plate';
SHOW INDEX FROM Vehicles WHERE Key_name = 'IX_Vehicles_Plate';
```

Resultado observado:

```text
Plate existe como varchar(10), Null = NO, Key = UNI.
IX_Vehicles_Plate existe como índice único sobre Plate.
```

No se hizo commit.

## Archivos modificados

```text
Application/Features/ClientVehicleFlows/ClientVehicleFlowService.cs
Application/Features/ClientVehicleFlows/Dtos/ClientVehicleDto.cs
Application/Features/ClientVehicleFlows/Dtos/ClientWithVehicleDto.cs
Application/Features/ClientVehicleFlows/Errors/ClientVehicleFlowErrors.cs
Application/Features/ClientVehicleFlows/Requests/AddVehicleToClientRequest.cs
Application/Features/ClientVehicleFlows/Requests/CreateClientWithVehicleRequest.cs
Application/Features/Search/Dtos/VehicleSearchResultDto.cs
Application/Features/Search/SearchService.cs
Application/Features/ServiceOrderWorkflow/Dtos/ServiceOrderFullDetailDto.cs
Application/Features/ServiceOrderWorkflow/ServiceOrderWorkflowService.cs
Application/Features/Vehicles/Dtos/VehicleDto.cs
Application/Features/Vehicles/Errors/VehicleErrors.cs
Application/Features/Vehicles/Requests/CreateVehicleRequest.cs
Application/Features/Vehicles/Requests/UpdateVehicleRequest.cs
Application/Features/Vehicles/VehicleService.cs
Domain/Entities/Vehicle.cs
Infrastructure/Persistence/Configurations/VehicleConfiguration.cs
Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs
```

## Archivos creados

```text
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.cs
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.Designer.cs
Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs
reporte-cambios-backend-placa-vehiculo.md
```

Nota: `20260602164728_AddVehiclePlate_Fix` es una migración vacía que quedó aplicada. La migración efectiva es `20260602170500_AddVehiclePlateColumn`.

## Archivos temporales eliminados durante la corrección

Durante el diagnóstico de migraciones se eliminaron archivos temporales/incorrectos que no quedaron en el repo:

```text
Infrastructure/Persistence/Migrations/20260602120000_AddVehiclePlate.cs
Infrastructure/Persistence/Migrations/20260602164301_AddVehiclePlate.cs
Infrastructure/Persistence/Migrations/20260602164301_AddVehiclePlate.Designer.cs
```

Motivo:

```text
20260602120000_AddVehiclePlate.cs existía sin su archivo Designer correspondiente y EF Core no la reconocía.
20260602164301_AddVehiclePlate.cs duplicaba la clase AddVehiclePlate y rompía la compilación.
```

## Cambios por capa

### Domain

Archivo:

```text
Domain/Entities/Vehicle.cs
```

Cambio:

```csharp
public string Plate { get; set; } = string.Empty;
```

Impacto:

```text
Vehicle ahora tiene placa como parte del modelo de dominio.
```

### Infrastructure / EF Core

Archivo:

```text
Infrastructure/Persistence/Configurations/VehicleConfiguration.cs
```

Cambios:

```text
Plate es requerido.
Plate tiene longitud máxima de 10.
Plate tiene índice único.
```

Configuración agregada:

```csharp
builder.Property(x => x.Plate)
    .IsRequired()
    .HasMaxLength(10);

builder.HasIndex(x => x.Plate)
    .IsUnique();
```

Archivo:

```text
Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs
```

Cambios:

```text
El snapshot ya refleja Plate varchar(10), requerido y con índice único.
```

### Application / Vehicles CRUD

Archivos:

```text
Application/Features/Vehicles/Requests/CreateVehicleRequest.cs
Application/Features/Vehicles/Requests/UpdateVehicleRequest.cs
Application/Features/Vehicles/Dtos/VehicleDto.cs
Application/Features/Vehicles/Errors/VehicleErrors.cs
Application/Features/Vehicles/VehicleService.cs
```

Cambios:

```text
CreateVehicleRequest ahora recibe Plate.
UpdateVehicleRequest ahora recibe Plate.
VehicleDto ahora devuelve Plate.
VehicleErrors incluye errores específicos para Plate.
VehicleService normaliza, valida, persiste y devuelve Plate.
```

Errores agregados:

```csharp
VehicleErrors.PlateRequired
VehicleErrors.PlateInvalid
VehicleErrors.PlateAlreadyExists
```

Reglas agregadas en `VehicleService`:

```text
Plate requerida.
Plate se normaliza con Trim + ToUpperInvariant.
Longitud mínima: 5.
Longitud máxima: 10.
Formato permitido: letras, números y guiones opcionales.
Duplicado bloqueado si existe otro vehículo activo con la misma placa.
En update se excluye el vehículo actual de la validación de duplicado.
```

Regex usada:

```csharp
^[A-Z0-9]+(?:-[A-Z0-9]+)*$
```

### Application / ClientVehicleFlows

Archivos:

```text
Application/Features/ClientVehicleFlows/Requests/CreateClientWithVehicleRequest.cs
Application/Features/ClientVehicleFlows/Requests/AddVehicleToClientRequest.cs
Application/Features/ClientVehicleFlows/Dtos/ClientWithVehicleDto.cs
Application/Features/ClientVehicleFlows/Dtos/ClientVehicleDto.cs
Application/Features/ClientVehicleFlows/Errors/ClientVehicleFlowErrors.cs
Application/Features/ClientVehicleFlows/ClientVehicleFlowService.cs
```

Cambios:

```text
CreateClientWithVehicleRequest ahora recibe Plate.
AddVehicleToClientRequest ahora recibe Plate.
ClientWithVehicleDto ahora devuelve Plate.
ClientVehicleDto ahora devuelve Plate.
ClientVehicleFlowErrors incluye errores específicos para Plate.
ClientVehicleFlowService normaliza, valida, persiste y devuelve Plate.
```

Errores agregados:

```csharp
ClientVehicleFlowErrors.PlateRequired
ClientVehicleFlowErrors.PlateInvalid
ClientVehicleFlowErrors.PlateAlreadyExists
```

Reglas agregadas en `ClientVehicleFlowService`:

```text
Plate requerida.
Plate se normaliza con Trim + ToUpperInvariant.
Longitud mínima: 5.
Longitud máxima: 10.
Formato permitido: letras, números y guiones opcionales.
Duplicado bloqueado si existe otro vehículo activo con la misma placa.
```

### Application / Search

Archivos:

```text
Application/Features/Search/Dtos/VehicleSearchResultDto.cs
Application/Features/Search/SearchService.cs
```

Cambios:

```text
VehicleSearchResultDto ahora devuelve Plate.
SearchService ahora busca vehículos también por Plate.
```

Impacto:

```text
GET /api/search/vehicles?term=ABC puede encontrar vehículos por placa.
```

### Application / ServiceOrderWorkflow

Archivos:

```text
Application/Features/ServiceOrderWorkflow/Dtos/ServiceOrderFullDetailDto.cs
Application/Features/ServiceOrderWorkflow/ServiceOrderWorkflowService.cs
```

Cambios:

```text
ServiceOrderFullDetailDto ahora incluye VehiclePlate.
ServiceOrderWorkflowService consulta el vehículo relacionado y mapea VehiclePlate.
```

Impacto:

```text
GET /api/service-orders/{id}/full-detail ahora puede devolver la placa del vehículo.
```

## Migraciones

### Migración vacía aplicada

Archivo:

```text
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.cs
```

Estado:

```text
Aplicada.
No ejecutó cambios de esquema.
Solo quedó registrada en __EFMigrationsHistory.
```

Motivo:

```text
EF la generó cuando el snapshot ya contenía Plate, por eso Up() y Down() quedaron vacíos.
```

Riesgo:

```text
No debe borrarse si la base ya tiene ese MigrationId en __EFMigrationsHistory.
En bases nuevas, se aplicará como no-op antes de la migración real.
```

### Migración real aplicada

Archivo:

```text
Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs
```

Estado:

```text
Aplicada correctamente.
```

Operaciones ejecutadas:

```sql
ALTER TABLE `Vehicles` ADD `Plate` varchar(10) NULL;
UPDATE `Vehicles` SET `Plate` = CONCAT('TMP', LPAD(`VehicleId`, 7, '0')) WHERE `Plate` IS NULL OR `Plate` = '';
ALTER TABLE `Vehicles` MODIFY COLUMN `Plate` varchar(10) NOT NULL;
CREATE UNIQUE INDEX `IX_Vehicles_Plate` ON `Vehicles` (`Plate`);
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260602170500_AddVehiclePlateColumn', '9.0.9');
```

Motivo del flujo nullable -> update -> not null:

```text
Permite migrar bases que ya tienen vehículos existentes.
Primero agrega Plate nullable, luego asigna placas temporales únicas, después convierte Plate a requerido y finalmente crea el índice único.
```

Placas temporales:

```text
Si existían vehículos previos, quedaron con placas tipo TMP0000001, TMP0000002, etc.
Estas placas deben ser reemplazadas por placas reales desde la app o por SQL controlado.
```

## Endpoints afectados

```text
POST /api/receptionist/create-client-with-vehicle
```

Cambio:

```text
El request ahora debe incluir plate.
El response devuelve Plate si el flujo crea correctamente cliente + vehículo.
```

```text
POST /api/clients/{personId}/vehicles
```

Cambio:

```text
El request ahora debe incluir plate.
El response devuelve Plate.
```

```text
POST /api/vehicles
```

Cambio:

```text
El request ahora debe incluir plate.
El response devuelve Plate.
```

```text
PUT /api/vehicles/{id}
```

Cambio:

```text
El request ahora debe incluir plate.
El response devuelve Plate.
```

```text
GET /api/vehicles
GET /api/vehicles/{id}
GET /api/clients/{personId}/vehicles
GET /api/client/my-vehicles
```

Cambio:

```text
Los DTOs de vehículo ahora incluyen Plate.
```

```text
GET /api/search/vehicles?term={term}
```

Cambio:

```text
La búsqueda considera Plate y el resultado devuelve Plate.
```

```text
GET /api/service-orders/{id}/full-detail
```

Cambio:

```text
El response ahora incluye VehiclePlate.
```

## Contrato de requests

### CreateClientWithVehicleRequest

Campo agregado:

```json
{
  "plate": "ABC123"
}
```

Ejemplo esperado:

```json
{
  "documentTypeId": 1,
  "documentNumber": "123456789",
  "firstName": "Cliente",
  "lastName": "Prueba",
  "email": "cliente.placa@test.com",
  "phoneNumber": "3001234567",
  "modelId": 1,
  "vehicleTypeId": 1,
  "plate": "ABC123",
  "vin": "3KPF54AD6TE123456",
  "year": 2026,
  "color": "Negro",
  "mileage": 0
}
```

### AddVehicleToClientRequest

Campo agregado:

```json
{
  "plate": "ABC123"
}
```

### CreateVehicleRequest

Campo agregado:

```json
{
  "plate": "ABC123"
}
```

### UpdateVehicleRequest

Campo agregado:

```json
{
  "plate": "ABC123"
}
```

Nota:

```text
En C# los request properties quedaron como string? para mantener estilo actual del proyecto, pero los servicios validan Plate como obligatorio.
```

## Contrato de responses

DTOs que ahora exponen `Plate`:

```text
Application/Features/Vehicles/Dtos/VehicleDto.cs
Application/Features/ClientVehicleFlows/Dtos/ClientVehicleDto.cs
Application/Features/ClientVehicleFlows/Dtos/ClientWithVehicleDto.cs
Application/Features/Search/Dtos/VehicleSearchResultDto.cs
```

DTO que ahora expone `VehiclePlate`:

```text
Application/Features/ServiceOrderWorkflow/Dtos/ServiceOrderFullDetailDto.cs
```

## Validaciones funcionales agregadas

Valores válidos esperados:

```text
ABC123
ABC-123
ABC12D
```

Valores inválidos esperados:

```text
null
""
"   "
"AB12"
"ABCDEFGHIJK"
"ABC 123"
"ABC@123"
```

Comportamiento esperado:

```text
Plate se guarda en mayúsculas.
Plate se guarda sin espacios externos.
Plate no puede repetirse entre vehículos activos.
Crear un vehículo con placa duplicada debe devolver error controlado, no 500.
Actualizar un vehículo puede conservar su propia placa.
Actualizar un vehículo a la placa activa de otro vehículo debe fallar.
```

## Documentación API

Se buscó documentación tipo:

```text
docs/api-contract.md
api-contract
contract
openapi
swagger
```

Resultado:

```text
No se encontró un archivo de documentación de contrato API equivalente en el repo.
Por eso no se modificó documentación adicional.
```

## Comandos relevantes usados

Build:

```powershell
dotnet build .\AutoTallerManager.slnx
```

Listar migraciones con override temporal de SSL:

```powershell
$env:ConnectionStrings__DefaultConnection='server=localhost;port=3306;database=AutoTallerManager;user=root;password=1234;SslMode=None;'
dotnet ef migrations list --no-build --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Aplicar migraciones con override temporal de SSL:

```powershell
$env:ConnectionStrings__DefaultConnection='server=localhost;port=3306;database=AutoTallerManager;user=root;password=1234;SslMode=None;'
dotnet ef database update --no-build --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Verificar columna e índice en MySQL:

```sql
USE AutoTallerManager;
SHOW COLUMNS FROM Vehicles LIKE 'Plate';
SHOW INDEX FROM Vehicles WHERE Key_name = 'IX_Vehicles_Plate';
```

Revisar placas temporales:

```sql
SELECT VehicleId, Plate
FROM Vehicles
WHERE Plate LIKE 'TMP%';
```

## Recomendaciones para el equipo

Mantener `20260602164728_AddVehiclePlate_Fix` aunque esté vacía, porque ya fue aplicada en la base local usada durante este trabajo.

No crear otra migración `AddVehiclePlate` con el mismo nombre de clase para evitar colisiones de compilación.

Antes de trabajar sobre este cambio, cada compañero debería correr:

```powershell
dotnet build .\AutoTallerManager.slnx
dotnet ef migrations list --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Si aparece error de SSL en MySQL local, usar temporalmente:

```powershell
$env:ConnectionStrings__DefaultConnection='server=localhost;port=3306;database=AutoTallerManager;user=root;password=1234;SslMode=None;'
```

Actualizar datos reales de placas temporales antes de pruebas funcionales o demos.

Probar manualmente:

```text
POST /api/receptionist/create-client-with-vehicle con plate válido.
POST /api/receptionist/create-client-with-vehicle con plate duplicado.
POST /api/vehicles con plate válido.
PUT /api/vehicles/{id} cambiando plate.
GET /api/search/vehicles?term=ABC.
GET /api/service-orders/{id}/full-detail.
```

## Estado git observado antes de crear este reporte

Archivos modificados:

```text
Application/Features/ClientVehicleFlows/ClientVehicleFlowService.cs
Application/Features/ClientVehicleFlows/Dtos/ClientVehicleDto.cs
Application/Features/ClientVehicleFlows/Dtos/ClientWithVehicleDto.cs
Application/Features/ClientVehicleFlows/Errors/ClientVehicleFlowErrors.cs
Application/Features/ClientVehicleFlows/Requests/AddVehicleToClientRequest.cs
Application/Features/ClientVehicleFlows/Requests/CreateClientWithVehicleRequest.cs
Application/Features/Search/Dtos/VehicleSearchResultDto.cs
Application/Features/Search/SearchService.cs
Application/Features/ServiceOrderWorkflow/Dtos/ServiceOrderFullDetailDto.cs
Application/Features/ServiceOrderWorkflow/ServiceOrderWorkflowService.cs
Application/Features/Vehicles/Dtos/VehicleDto.cs
Application/Features/Vehicles/Errors/VehicleErrors.cs
Application/Features/Vehicles/Requests/CreateVehicleRequest.cs
Application/Features/Vehicles/Requests/UpdateVehicleRequest.cs
Application/Features/Vehicles/VehicleService.cs
Domain/Entities/Vehicle.cs
Infrastructure/Persistence/Configurations/VehicleConfiguration.cs
Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs
```

Archivos no trackeados:

```text
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.cs
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.Designer.cs
Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs
```

Después de crear este reporte, también queda no trackeado:

```text
reporte-cambios-backend-placa-vehiculo.md
```
