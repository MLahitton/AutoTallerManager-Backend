# Notas: migración al agregar campo

## Referencia real

`Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs`

Esa migración:

1. Agrega columna nullable.
2. Rellena valores para filas existentes (`UPDATE ... SET Plate = CONCAT('TMP', ...)`).
3. Altera columna a `NOT NULL`.
4. Crea índice único.

## Comando

```bash
dotnet ef migrations add AddNewFieldToEntity --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

## Aplicar

```bash
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

## Checklist

- [ ] Propiedad en entidad Domain.
- [ ] DTO y Requests actualizados.
- [ ] Validación en Service.
- [ ] Errores nuevos con sufijos correctos.
- [ ] Configuration EF actualizada.
- [ ] Migración creada y revisada.
- [ ] `dotnet build` OK.
- [ ] Swagger POST/PUT con el campo nuevo.

## Si el campo es opcional

La migración puede ser más simple: `AddColumn` nullable sin paso de backfill.

## No crear migración si...

Solo cambias lógica de servicio sin tocar el esquema.
