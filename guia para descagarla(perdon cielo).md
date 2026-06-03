# Guia Para Continuar Con Los Cambios De Placa Vehicular

## 1. Actualizar El Proyecto

Desde la carpeta del backend:

```powershell
git pull
```

## 2. Compilar El Proyecto

```powershell
dotnet build .\AutoTallerManager.slnx
```

El resultado esperado es:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

## 3. Aplicar Migraciones

```powershell
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

## 4. Si Sale Error De SSL En MySQL

Ejecutar este comando primero:

```powershell
$env:ConnectionStrings__DefaultConnection='server=localhost;port=3306;database=AutoTallerManager;user=root;password=1234;SslMode=None;'
```

Luego repetir:

```powershell
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

## 5. Verificar En MySQL Workbench

```sql
USE AutoTallerManager;

SHOW COLUMNS FROM Vehicles LIKE 'Plate';

SHOW INDEX FROM Vehicles WHERE Key_name = 'IX_Vehicles_Plate';
```

Resultado esperado:

```text
La columna Plate debe existir.
Plate debe ser varchar(10).
Plate debe tener Null = NO.
Debe existir el indice unico IX_Vehicles_Plate.
```

## 6. Confirmar Migraciones En EF

```powershell
dotnet ef migrations list --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Debe aparecer algo parecido a:

```text
20260528114245_InitialCreate
20260602164728_AddVehiclePlate_Fix
20260602170500_AddVehiclePlateColumn
```

## 7. Leer El Reporte De Contexto

Revisar este archivo del proyecto:

```text
reporte-cambios-backend-placa-vehiculo.md
```

Ese reporte explica:

```text
Que archivos se modificaron.
Que migraciones se crearon.
Que endpoints cambiaron.
Que validaciones se agregaron.
Que debe revisar el equipo.
```

## 8. Revisar Placas Temporales

Si ya existian vehiculos en la base de datos, la migracion pudo asignar placas temporales como:

```text
TMP0000001
TMP0000002
TMP0000003
```

Para revisarlas:

```sql
SELECT VehicleId, Plate
FROM Vehicles
WHERE Plate LIKE 'TMP%';
```

Esas placas pueden reemplazarse luego por las placas reales desde la app o mediante SQL controlado.

## 9. Endpoints Que Conviene Probar

```text
POST /api/receptionist/create-client-with-vehicle
POST /api/clients/{personId}/vehicles
POST /api/vehicles
PUT /api/vehicles/{id}
GET /api/search/vehicles?term=ABC
GET /api/service-orders/{id}/full-detail
```

## 10. Nota Importante

No eliminar estas migraciones:

```text
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.cs
Infrastructure/Persistence/Migrations/20260602164728_AddVehiclePlate_Fix.Designer.cs
Infrastructure/Persistence/Migrations/20260602170500_AddVehiclePlateColumn.cs
```

Aunque `20260602164728_AddVehiclePlate_Fix` este vacia, debe conservarse porque puede quedar registrada en `__EFMigrationsHistory`.
