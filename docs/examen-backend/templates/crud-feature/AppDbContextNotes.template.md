# Notas: AppDbContext para entidad CRUD nueva

## Archivo a modificar

`Infrastructure/Persistence/AppDbContext.cs`

## Qué agregar

Dentro de la clase `AppDbContext`, junto a los demás `DbSet`:

```csharp
public DbSet<NewEntity> NewEntities => Set<NewEntity>();
```

## No olvidar

1. `using Domain.Entities;` ya está en el archivo.
2. La configuración Fluent API va en `Configurations/NewEntityConfiguration.cs` (no en OnModelCreating manualmente).
3. `ApplyConfigurationsFromAssembly` ya registra todas las configuraciones del ensamblado Infrastructure.

## Referencia

Ver líneas de `DbSet<Supplier>`, `DbSet<Gender>`, etc. en `AppDbContext.cs`.

## Después de agregar DbSet

Crear migración:

```bash
dotnet ef migrations add AddNewEntity --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```
