# Notas: migración para entidad CRUD nueva

## Cuándo crear migración

Solo si agregas o modificas el esquema de base de datos (tabla nueva, columna, índice, FK).

## Comandos

```bash
# Crear migración
dotnet ef migrations add AddNewEntity --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext

# Aplicar a la base de datos local
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

## Ubicación de archivos generados

`Infrastructure/Persistence/Migrations/`

## Referencia de migración existente

- Inicial: `20260528114245_InitialCreate.cs`
- Campo nuevo: `20260602170500_AddVehiclePlateColumn.cs`
- Cancelación compra: `20260603163044_AddPartPurchaseCancellation.cs`

## Buenas prácticas en examen

1. Nombre descriptivo: `AddNewEntity`, `AddPlateToVehicle`.
2. Revisa el archivo generado antes de aplicar.
3. Si la API está corriendo, deténla antes de `dotnet build` o `ef database update`.
4. No edites manualmente el snapshot salvo que sepas lo que haces; deja que EF lo genere.

## Seeder opcional

Si el enunciado pide datos iniciales, agrega en `DatabaseSeeder.cs` o crea un seeder dedicado (ver `DemoAccountsSeeder.cs`).
