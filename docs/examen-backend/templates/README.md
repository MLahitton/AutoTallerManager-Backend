# Plantillas de estudio (`.template.cs`)

Estas plantillas **no se compilan**. Sirven para copiar la estructura correcta durante el examen.

## Carpetas

| Carpeta | Cuándo usarla |
|---------|---------------|
| `crud-feature/` | Nueva entidad con CRUD completo |
| `business-action/` | Endpoint de acción (cancelar, emitir, aprobar, etc.) |
| `role-scoped-feature/` | Endpoints filtrados por Client / Mechanic / Admin |
| `add-field-to-entity/` | Agregar columna/campo a entidad existente |
| `audit-enabled-action/` | Flujo con `IAuditLogger` |

## Cómo copiar al proyecto real

1. Elige la carpeta según el tipo de tarea.
2. Abre el archivo `.template.cs` y la referencia indicada en los comentarios.
3. Crea los archivos `.cs` reales en las rutas del proyecto (sin `.template`).
4. Ajusta namespaces según la carpeta destino.
5. Registra el servicio en `Application/DependencyInjection.cs` si es nuevo.
6. Crea migración solo si cambia el esquema.

## Convención de placeholders

| Placeholder | Reemplazar por |
|-------------|----------------|
| `NewEntity` | Nombre de tu entidad (singular PascalCase) |
| `NewEntities` | Plural del recurso |
| `NewEntityId` | Clave primaria |
| `NewEntityService` | Nombre del servicio |
| `NewFeature` | Nombre del módulo de negocio |

## Referencias rápidas del proyecto

- CRUD simple: `Application/Features/Genders/`
- CRUD con validación: `Application/Features/Vehicles/`
- CRUD con auditoría: `Application/Features/Suppliers/`
- Acción de negocio: `Application/Features/InventoryBusiness/`
- Por rol Client: `Api/Controllers/ClientApprovalsController.cs`
- Por rol Mechanic: `Api/Controllers/MechanicWorkflowController.cs`
