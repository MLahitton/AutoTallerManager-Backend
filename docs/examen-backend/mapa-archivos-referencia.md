# Mapa de archivos de referencia

Usa esta tabla para encontrar rápidamente **dónde copiar el patrón** según lo que te pidan en el examen. Todas las rutas son relativas a la raíz de `AutoTallerManager-Backend`.

| Si necesito hacer... | Me puedo guiar de... | Por qué sirve |
| -------------------- | -------------------- | ------------- |
| CRUD simple (catálogo) | `Application/Features/Genders/GenderService.cs`, `Api/Controllers/GendersController.cs` | CRUD completo sin auditoría ni claims; validación en servicio |
| CRUD con validación de negocio | `Application/Features/Vehicles/VehicleService.cs`, `Application/Features/Vehicles/Errors/VehicleErrors.cs` | Validación de placa/VIN, unicidad, FKs, `InUse` al eliminar |
| Agregar campo a entidad | `Domain/Entities/Vehicle.cs` (campo `Plate`), `Infrastructure/Persistence/Configurations/VehicleConfiguration.cs`, migración `Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs` | Patrón real de columna nueva con índice único |
| Acción de negocio (endpoint) | `Application/Features/InventoryBusiness/InventoryBusinessService.cs`, `Api/Controllers/InventoryBusinessController.cs` | Acciones POST con rutas de negocio, no CRUD |
| Operación transaccional | `InventoryBusinessService.RegisterPurchaseAsync`, `SupplierService.CreateAsync` (usa `ExecuteInTransactionAsync`) | Varias tablas + `SaveChanges` dentro de transacción |
| Registro de auditoría | `Application/Common/Auditing/IAuditLogger.cs`, `SupplierService.cs`, `InventoryBusinessService.CancelPurchaseAsync` | `LogAsync` + segundo `SaveChanges` en transacción |
| Endpoint scoped para Client | `Api/Controllers/ClientVehiclesController.cs`, `Application/Features/ClientApprovals/ClientApprovalService.cs` | `personId` desde JWT; filtro por propiedad del cliente |
| Endpoint scoped para Mechanic | `Api/Controllers/MechanicWorkflowController.cs`, `Application/Features/ServiceExecution/ServiceExecutionService.cs` | Solo servicios asignados al `personId` del mecánico |
| Endpoint agregado Admin | `Api/Controllers/AdminMechanicsController.cs`, `Application/Features/AdminMechanics/AdminMechanicsService.cs` | Solo `[Authorize(Roles = "Admin")]` |
| Flujo de facturación | `Application/Features/InvoiceBusiness/InvoiceBusinessService.cs`, `Api/Controllers/InvoiceBusinessController.cs` | Generar factura desde orden, estados, conflicto duplicado |
| Flujo de pagos | `Application/Features/PaymentBusiness/PaymentBusinessService.cs`, `Api/Controllers/PaymentBusinessController.cs` | Registrar pago, actualizar factura, reembolso |
| Flujo de compras | `Application/Features/InventoryBusiness/InventoryBusinessService.cs`, `Application/Features/PartPurchases/PartPurchaseService.cs` | Compra + detalle + stock |
| Auditoría solo lectura | `Api/Controllers/AuditsController.cs`, `Application/Features/Audits/AuditService.cs` | GET sin mutación |
| Consulta de auditoría Admin | `Api/Controllers/AdminAuditQueriesController.cs`, `Application/Features/AuditQueries/AuditQueryService.cs` | Filtros `recent`, `by-entity`, `by-user` |
| Reporte / query read-only | `Api/Controllers/ReportsController.cs`, `Application/Features/Reports/ReportService.cs` | GET con filtros `from`/`to`, sin cambios en BD |
| Seeder | `Infrastructure/Persistence/Seeders/DatabaseSeeder.cs`, `DemoAccountsSeeder.cs` | Datos iniciales y cuentas demo |
| Configuración EF | `Infrastructure/Persistence/Configurations/GenderConfiguration.cs`, `VehicleConfiguration.cs` | Tabla, clave, índices, relaciones |
| Patrón de migración | `Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs` | `AddColumn`, índice, datos existentes |
| Controller con Result/Error | `Api/Controllers/BaseApiController.cs`, cualquier controller que herede de él | `FromResult` centraliza HTTP |
| Endpoint de búsqueda | `Api/Controllers/SearchController.cs`, `Application/Features/Search/SearchService.cs` | GET con `term`, límite de resultados |
| Patrón Result/Error | `Application/Common/Results/Result.cs`, `Application/Features/Vehicles/Errors/VehicleErrors.cs` | Errores estáticos con sufijos que mapean a HTTP |
| Unit of Work | `Application/Common/Interfaces/Persistence/IUnitOfWork.cs`, `Infrastructure/Persistence/UnitOfWork.cs` | Repositorio genérico y transacciones |
| AppDbContext | `Infrastructure/Persistence/AppDbContext.cs` | Todos los `DbSet<>` |
| Registro DI | `Application/DependencyInjection.cs` | `AddScoped<IService, Service>()` |
| JWT y claims | `Api/Security/JwtTokenGenerator.cs`, `Api/Controllers/AccountController.cs` | Claims `userId`, `personId`, roles |
| Login / auth | `Api/Controllers/AuthController.cs`, `Application/Features/Auth/AuthService.cs` | Endpoints públicos de autenticación |
| Aprobaciones cliente | `Api/Controllers/ClientApprovalsController.cs`, `ClientApprovalService.cs` | Approve/reject con ownership |
| Ejecución de servicio | `ServiceExecutionService.cs`, `ServiceExecutionErrors.cs` | Mecánico no asignado → Conflict |
| CRUD con auditoría en mutaciones | `Application/Features/Suppliers/SupplierService.cs` | CREATE/UPDATE/DELETE auditan |
| Cancelar compra (protección) | `InventoryBusinessService.CancelPurchaseAsync`, `InventoryBusinessErrors.PurchaseAlreadyCancelledConflict` | Regla 409 en compra cancelada |
| Órdenes de servicio | `Application/Features/ServiceOrders/ServiceOrderService.cs`, `Api/Controllers/ServiceOrdersController.cs` | Entidad central del taller |
| Servicios en orden | `Application/Features/OrderServices/OrderServiceService.cs` | Líneas de servicio |
| Partes en servicio | `Application/Features/OrderServiceParts/OrderServicePartService.cs` | Repuestos por servicio |
| Cuenta del usuario | `Api/Controllers/AccountController.cs`, `Application/Features/Account/AccountService.cs` | Perfil con `userId` + `personId` del token |

## Estructura típica de una feature

```txt
Application/Features/{NombreFeature}/
  {NombreFeature}Service.cs
  I{NombreFeature}Service.cs          (opcional pero habitual)
  Dtos/
    {Entidad}Dto.cs
  Requests/
    Create{Entidad}Request.cs
    Update{Entidad}Request.cs
  Errors/
    {Entidad}Errors.cs

Api/Controllers/
  {Entidades}Controller.cs

Domain/Entities/
  {Entidad}.cs

Infrastructure/Persistence/
  Configurations/{Entidad}Configuration.cs
  Migrations/...
```

## Features de negocio separadas del CRUD

Algunos dominios tienen **dos capas**: CRUD básico + servicio de negocio.

| Dominio | CRUD | Negocio |
|---------|------|---------|
| Inventario / compras | `PartPurchasesController`, `PartPurchaseService` | `InventoryBusinessController`, `InventoryBusinessService` |
| Facturas | `InvoicesController`, `InvoiceService` | `InvoiceBusinessController`, `InvoiceBusinessService` |
| Pagos | `PaymentsController`, `PaymentService` | `PaymentBusinessController`, `PaymentBusinessService` |
| Órdenes | `ServiceOrdersController` | `ServiceOrderWorkflowController`, `WorkshopIntakeController` |

En el examen, si piden **cancelar**, **emitir**, **aprobar** o **registrar pago**, casi siempre corresponde al servicio de **negocio**, no al CRUD simple.
