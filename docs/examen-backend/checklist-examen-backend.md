# Checklist rápido para modificar backend

## Antes de codificar

- [ ] Entender el enunciado (¿qué endpoint, qué rol, qué datos?).
- [ ] Buscar una feature similar (`mapa-archivos-referencia.md`).
- [ ] Identificar capas afectadas (Domain, Application, Api, Infrastructure).
- [ ] Verificar si hay cambio de base de datos (entidad nueva, campo, relación).
- [ ] Verificar roles requeridos (`[Authorize(Roles = "...")]`).
- [ ] Verificar si el flujo exige auditoría (`IAuditLogger`).

## Durante la codificación

- [ ] Entidad de dominio (`Domain/Entities/`).
- [ ] DTO (`Application/Features/.../Dtos/`).
- [ ] Request (`Application/Features/.../Requests/`).
- [ ] Errors (`Application/Features/.../Errors/`).
- [ ] Service + interfaz (`Application/Features/.../`).
- [ ] Controller (`Api/Controllers/`).
- [ ] `DbSet` en `AppDbContext` (si aplica).
- [ ] `IEntityTypeConfiguration` (si aplica).
- [ ] Migración EF (solo si cambia esquema).
- [ ] Seeder (solo si el enunciado lo pide).
- [ ] Registro DI en `Application/DependencyInjection.cs` (servicio nuevo).

## Después de codificar

- [ ] `dotnet build .\AutoTallerManager.slnx` sin errores.
- [ ] Aplicar migración si se creó (`dotnet ef database update ...`).
- [ ] Ejecutar API (`dotnet run --project Api\Api.csproj`).
- [ ] Probar en Swagger (`http://localhost:5077/swagger`).
- [ ] Login con el rol correcto.
- [ ] Caso de éxito probado.
- [ ] Caso de error / validación probado.
- [ ] Verificar datos en base de datos si aplica.
- [ ] Verificar registro de auditoría si aplica.
- [ ] Poder explicar qué archivos tocaste y por qué.
