# Paquete de preparación para examen — Backend AutoTallerManager

## ¿Qué es este paquete?

Este directorio contiene **material de estudio y plantillas de referencia** para preparar modificaciones del backend durante un examen práctico. **No implementa funcionalidad real** ni modifica el código fuente del proyecto (`Api`, `Application`, `Domain`, `Infrastructure`).

Los archivos con extensión `.template.cs` son **plantillas para copiar y adaptar** durante el examen. Están fuera de los proyectos `.csproj`, por lo que **no se compilan** con la solución.

## Cómo usarlo durante el examen

1. Lee el enunciado y clasifica el cambio (CRUD, acción de negocio, campo nuevo, rol, auditoría, etc.).
2. Abre `checklist-examen-backend.md` como guía rápida.
3. Consulta `mapa-archivos-referencia.md` para ubicar un ejemplo real similar en el repositorio.
4. Sigue `guia-modificaciones-backend.md` según el tipo de tarea.
5. Copia la plantilla correspondiente de `templates/` y adapta nombres, rutas y validaciones.
6. Prueba con Swagger siguiendo `guia-pruebas-swagger.md`.
7. Valida con `dotnet build`.

## Resumen de arquitectura del backend

El repositorio sigue **Clean Architecture** con cuatro proyectos:

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| HTTP | `Api` | Controllers, JWT, Swagger, mapeo HTTP ↔ Result |
| Aplicación | `Application` | Services, DTOs, Requests, Errors, interfaces |
| Dominio | `Domain` | Entidades puras (sin EF ni ASP.NET) |
| Infraestructura | `Infrastructure` | EF Core, `AppDbContext`, configuraciones, repositorios, migraciones, seeders |

### Flujo típico de una petición

```
Cliente HTTP → Controller → Service → IUnitOfWork / Repository → EF → MySQL
                    ↓
              BaseApiController.FromResult → HTTP 200/201/204/400/404/409/403
```

### Patrones clave del proyecto

- **Result / Error**: los servicios devuelven `Result` o `Result<T>`; los errores son `Error` con `Code` y `Message`.
- **BaseApiController**: traduce sufijos del `Code` a HTTP (`NotFound` → 404, `Conflict` → 409, etc.).
- **IUnitOfWork**: acceso a datos genérico vía `Repository<T>()`, `SaveChangesAsync`, `ExecuteInTransactionAsync`.
- **Features por carpeta**: cada módulo vive en `Application/Features/{Nombre}/` con subcarpetas `Dtos`, `Requests`, `Errors`.
- **JWT**: claims `userId`, `personId`, `email` y roles en `ClaimTypes.Role`.
- **Auditoría**: `IAuditLogger.LogAsync` dentro de transacciones; la API de auditoría es de solo lectura.

### Roles del sistema

- **Admin**: acceso amplio, reportes, cancelaciones sensibles.
- **Receptionist**: operación diaria del taller (vehículos, inventario, órdenes).
- **Mechanic**: servicios asignados, ejecución de trabajo.
- **Client**: datos propios (vehículos, aprobaciones, facturas).

## Orden recomendado al agregar algo nuevo

```txt
1. Entender el cambio solicitado.
2. Identificar si es CRUD, acción de negocio, feature por rol, campo nuevo o flujo con auditoría.
3. Localizar una feature similar existente.
4. Crear o actualizar entidad de dominio si hace falta.
5. Crear o actualizar DTOs.
6. Crear o actualizar Requests.
7. Crear o actualizar Errors.
8. Crear o actualizar servicio de aplicación.
9. Crear o actualizar interfaz del servicio si el proyecto la usa en ese caso.
10. Crear o actualizar Controller.
11. Actualizar AppDbContext / Configuration si cambia la base de datos.
12. Crear migración solo si cambia el esquema.
13. Registrar en DI (`Application/DependencyInjection.cs`) solo si es un servicio nuevo.
14. Probar con Swagger.
15. Validar con `dotnet build`.
```

## Contenido de este paquete

| Archivo | Propósito |
|---------|-----------|
| `guia-modificaciones-backend.md` | Guía completa por tipo de cambio |
| `guia-pruebas-swagger.md` | Cómo probar en Swagger por rol y caso |
| `mapa-archivos-referencia.md` | Tabla tarea → archivo real de referencia |
| `checklist-examen-backend.md` | Lista rápida antes / durante / después |
| `templates/` | Plantillas `.template.cs` y notas `.md` |

## Advertencias importantes

- **No inventes arquitectura**: copia el estilo de features existentes (`Vehicles`, `Genders`, `InventoryBusiness`, etc.).
- **No confíes en `userId` / `personId` del body**: extráelos del JWT en el controller.
- **Migración solo si cambia el esquema** de la base de datos.
- **Detén la API** antes de `dotnet build` si los DLL están bloqueados.
