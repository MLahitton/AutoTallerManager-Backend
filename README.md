# AutoTallerManager Backend

Backend RESTful para AutoTallerManager, un sistema de gestion integral para un taller automotriz moderno.

Este repositorio contiene la API y la logica backend del sistema. Expone servicios HTTP para operar clientes, vehiculos, ordenes de servicio, mecanicos, inventario, compras, facturacion, pagos, aprobaciones del cliente, auditoria y dashboards por rol.

**Frontend:** https://github.com/ximenaa19/AutoTallerManager-Fronted

## Tabla de contenidos

- [Descripcion general](#descripcion-general)
- [Arquitectura](#arquitectura)
- [Stack tecnico](#stack-tecnico)
- [Modulos principales](#modulos-principales)
- [Roles del sistema](#roles-del-sistema)
- [Flujos de negocio principales](#flujos-de-negocio-principales)
- [Requisitos previos](#requisitos-previos)
- [Configuracion](#configuracion)
- [Base de datos y migraciones](#base-de-datos-y-migraciones)
- [Seeders de datos iniciales](#seeders-de-datos-iniciales)
- [Como ejecutar el backend](#como-ejecutar-el-backend)
- [Swagger y documentacion de API](#swagger-y-documentacion-de-api)
- [Endpoints relevantes por dominio](#endpoints-relevantes-por-dominio)
- [Validacion rapida](#validacion-rapida)
- [Relacion con el frontend](#relacion-con-el-frontend)
- [Buenas practicas](#buenas-practicas)
- [Troubleshooting](#troubleshooting)
- [Estado del proyecto](#estado-del-proyecto)

## Descripcion general

AutoTallerManager Backend centraliza la logica de negocio de un taller automotriz. Su objetivo es ofrecer una API clara para que el frontend pueda operar los flujos de los perfiles Admin, Receptionist, Mechanic y Client.

El backend cubre flujos relacionados con:

- Clientes.
- Vehiculos.
- Ordenes de servicio.
- Mecanicos y asignaciones.
- Ejecucion de servicios.
- Inventario de repuestos.
- Compras de inventario.
- Cancelacion o reverso de compras.
- Facturacion.
- Pagos.
- Aprobaciones pendientes del cliente.
- Auditoria y trazabilidad.
- Dashboards por rol.

## Arquitectura

El repositorio usa una separacion por capas/proyectos, alineada con principios de Clean Architecture y arquitectura hexagonal.

### `Api`

Proyecto ASP.NET Core encargado de la capa HTTP.

Responsabilidades principales:

- Configuracion de la aplicacion.
- Controllers REST.
- Autenticacion JWT Bearer.
- Autorizacion.
- Swagger/OpenAPI.
- CORS para frontends locales.
- Arranque del seeder runtime.
- Bootstrap de administrador en Development.

### `Application`

Capa de aplicacion y casos de uso.

Responsabilidades principales:

- Servicios de aplicacion.
- DTOs.
- Reglas de negocio.
- Contratos de seguridad.
- Contratos de persistencia.
- Validaciones con FluentValidation.
- Patron de resultados mediante `Result` y `Result<T>`.
- Inyeccion de dependencias de servicios de negocio.

### `Domain`

Capa de dominio.

Responsabilidades principales:

- Entidades principales del negocio.
- Modelo central del taller.
- Conceptos persistidos como usuarios, personas, vehiculos, ordenes, servicios, repuestos, facturas, pagos, auditorias y catalogos.

### `Infrastructure`

Capa de infraestructura y persistencia.

Responsabilidades principales:

- `AppDbContext`.
- Configuraciones de Entity Framework Core.
- Repositorio generico.
- Unit of Work.
- Migraciones EF Core.
- Seeders de datos maestros y catalogos.
- Integracion con MySQL mediante Pomelo Entity Framework Core Provider.

## Stack tecnico

- C#.
- .NET SDK configurado en `global.json`: `10.0.202`.
- Target framework de los proyectos: `net10.0`.
- ASP.NET Core.
- Entity Framework Core `9.0.9`.
- Pomelo.EntityFrameworkCore.MySql `9.0.0`.
- MySQL como motor de base de datos.
- JWT Bearer Authentication.
- Swagger/OpenAPI con Swashbuckle `10.1.7`.
- FluentValidation `12.1.1`.
- Inyeccion de dependencias nativa de ASP.NET Core.
- Solucion en formato `.slnx`: `AutoTallerManager.slnx`.

## Modulos principales

### Auth / Account

Gestiona registro de clientes, login, refresh token, logout, perfil actual y cambio de contrasena.

### Dashboard

Expone resumenes por rol para Client, Mechanic, Receptionist y Admin.

### Admin

Agrupa capacidades administrativas como usuarios, roles, mecanicos, reportes, catalogos, auditoria e informacion global del taller.

### Customers / Clients

Permite gestionar personas/clientes y flujos especificos de recepcion como creacion de cliente con vehiculo.

### Vehicles

Gestiona vehiculos, tipos, marcas, modelos, historiales de propietario e inventario de ingreso del vehiculo.

### Service Orders

Permite crear, consultar, actualizar y administrar ordenes de servicio del taller.

### Service Order Workflow

Gestiona cambios de estado, cancelacion, anulacion, finalizacion y consulta detallada de ordenes.

### Mechanic / Service Execution

Soporta asignaciones de mecanicos, consulta de servicios asignados, ordenes activas, registro de trabajo y solicitud de repuestos.

### Inventory

Administra repuestos, categorias, marcas, proveedores y consultas operativas de inventario.

### Purchases

Permite registrar compras de repuestos, detalles de compra y flujos de cancelacion/reversion cuando aplica.

### Invoices

Gestiona facturas, estados de factura y operaciones de negocio relacionadas con generacion y consulta.

### Invoice Details

Gestiona los detalles asociados a facturas.

### Payments

Gestiona pagos, metodos de pago, estados de pago y tarjetas asociadas.

### Client Approvals

Permite que el cliente apruebe o rechace servicios y repuestos pendientes.

### Audit

Expone auditorias, tipos de accion y consultas administrativas de trazabilidad.

### Catalogs / Master Data

Centraliza catalogos publicos y catalogos de taller necesarios para formularios y operaciones del frontend.

## Roles del sistema

### Admin

Puede gestionar administracion general, usuarios, roles, catalogos, clientes, vehiculos, ordenes, inventario, compras, mecanicos, facturacion, pagos, reportes y auditoria.

### Receptionist

Puede operar recepcion: clientes, vehiculos, ordenes, inventario operativo, compras, facturas y pagos. No debe tener permisos administrativos globales.

### Mechanic

Puede ver sus servicios asignados, ordenes activas, registrar trabajo y solicitar repuestos.

### Client

Puede ver su portal, vehiculos, ordenes, facturas y aprobar o rechazar servicios o repuestos pendientes.

## Flujos de negocio principales

### Flujo recepcion a cliente a factura

1. Receptionist crea cliente y vehiculo si aplica.
2. Receptionist crea orden de servicio.
3. Client aprueba servicios o repuestos pendientes.
4. Receptionist genera factura.
5. Receptionist registra pago.
6. Client consulta factura y resumen de pago.

### Flujo mecanico

1. Admin o responsable asigna mecanico.
2. Mechanic consulta servicios asignados.
3. Mechanic registra trabajo.
4. Mechanic solicita repuestos si aplica.
5. Client aprueba o rechaza si corresponde.
6. La orden continua su ciclo operativo.

### Flujo inventario

1. Receptionist o Admin consulta inventario.
2. Se registran compras.
3. El stock aumenta.
4. Admin puede cancelar o revertir compra si corresponde.
5. El sistema mantiene trazabilidad.

## Requisitos previos

- .NET SDK `10.0.202` o compatible con `net10.0`.
- MySQL disponible localmente o en un servidor accesible.
- EF Core CLI si se van a crear o aplicar migraciones.
- IDE recomendado: Visual Studio, Rider o Visual Studio Code.
- Acceso al repositorio del frontend si se quiere probar el flujo completo.

Instalacion de EF Core CLI si no esta disponible:

```powershell
dotnet tool install --global dotnet-ef
```

Actualizacion de EF Core CLI si ya existe:

```powershell
dotnet tool update --global dotnet-ef
```

## Configuracion

La configuracion principal vive en:

- `Api/appsettings.json`.
- `Api/appsettings.Development.json`.
- Variables de entorno del sistema o del entorno de despliegue.

Configuraciones relevantes:

- `ConnectionStrings:DefaultConnection`: conexion a MySQL.
- `Jwt:Issuer`: emisor esperado del token.
- `Jwt:Audience`: audiencia esperada del token.
- `Jwt:SecretKey`: clave usada para firmar tokens JWT.
- `Jwt:AccessTokenExpirationMinutes`: expiracion del access token.
- `Jwt:RefreshTokenExpirationDays`: expiracion del refresh token.
- `SeedData:Enabled`: controla el seeder runtime.
- `BootstrapAdmin`: configuracion local de bootstrap de administrador en Development.

Importante:

- No versionar secretos reales.
- No subir credenciales productivas.
- Usar variables de entorno para credenciales, secretos JWT y cadenas de conexion reales.
- Mantener `appsettings.Development.json` solo para valores de desarrollo local.

Ejemplo generico de variables de entorno:

```powershell
$env:ConnectionStrings__DefaultConnection="server=HOST;port=3306;database=DB_NAME;user=DB_USER;password=DB_PASSWORD;"
$env:Jwt__SecretKey="REPLACE_WITH_A_LONG_SECURE_SECRET_KEY"
```

## Base de datos y migraciones

El proyecto usa Entity Framework Core con MySQL.

Migraciones existentes detectadas:

- `20260528114245_InitialCreate`.
- `20260602164728_AddVehiclePlate_Fix`.
- `20260602170500_AddVehiclePlateColumn`.
- `20260603163044_AddPartPurchaseCancellation`.

Compilar la solucion:

```powershell
dotnet build .\AutoTallerManager.slnx
```

Crear una nueva migracion:

```powershell
dotnet ef migrations add NombreMigracion --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Aplicar migraciones pendientes:

```powershell
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Listar migraciones:

```powershell
dotnet ef migrations list --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Recomendacion:

- Revisar cuidadosamente cualquier migracion antes de aplicarla.
- No ejecutar `database update` contra una base compartida sin coordinacion del equipo.

## Seeders de datos iniciales

El backend tiene seeders runtime idempotentes para catalogos y datos maestros.

El seeder se ejecuta durante el arranque de la API cuando:

- El entorno es `Development` y `SeedData:Enabled` no esta en `false`.
- O cuando `SeedData:Enabled` esta en `true` explicitamente.

Catalogos y datos maestros cubiertos:

- Roles/perfiles base.
- Tipos de documento.
- Departamentos y ciudades.
- Tipos de vehiculo.
- Marcas y modelos de vehiculo.
- Estados de ordenes.
- Estados de factura.
- Estados de pago.
- Metodos de pago.
- Tipos de tarjeta.
- Tipos de servicio.
- Especialidades mecanicas.
- Categorias de repuestos.
- Marcas de repuestos.
- Repuestos iniciales.
- Proveedores base.
- Tipos de accion/auditoria.

El seeder no crea datos operativos:

- Usuarios reales.
- Personas.
- Clientes.
- Empleados.
- Vehiculos de clientes.
- Ordenes.
- Compras operativas.
- Facturas.
- Pagos.
- Logs transaccionales.

Comportamiento esperado:

- Puede ejecutarse varias veces sin duplicar los datos base.
- Agrega datos faltantes.
- No elimina datos existentes.
- No reemplaza datos productivos.

## Como ejecutar el backend

Clonar el repositorio:

```powershell
git clone https://github.com/MLahitton/AutoTallerManager-Backend
cd AutoTallerManager-Backend
```

Restaurar dependencias:

```powershell
dotnet restore
```

Compilar:

```powershell
dotnet build .\AutoTallerManager.slnx
```

Ejecutar la API:

```powershell
dotnet run --project Api\Api.csproj
```

URL local detectada en `launchSettings.json`:

```txt
http://localhost:5077
```

Perfil HTTPS local detectado:

```txt
https://localhost:7072
```

## Swagger y documentacion de API

Swagger esta habilitado en entorno `Development`.

URL esperada:

```txt
http://localhost:5077/swagger
```

Si se ejecuta con HTTPS:

```txt
https://localhost:7072/swagger
```

La configuracion de Swagger incluye soporte para JWT Bearer. En Swagger se debe ingresar solo el token, sin escribir manualmente la palabra `Bearer`.

## Endpoints relevantes por dominio

Esta lista resume endpoints reales observados en controllers. No es una lista exhaustiva de toda la API.

### Auth y cuenta

- `POST /api/auth/register-client`.
- `POST /api/auth/login`.
- `POST /api/auth/refresh`.
- `POST /api/auth/logout`.
- `GET /api/account/me`.
- `PUT /api/account/me`.
- `POST /api/account/change-password`.

### Catalogos

- `GET /api/catalogs/public-registration`.
- `GET /api/catalogs/workshop`.
- `GET /api/roles`.
- `GET /api/document-types`.
- `GET /api/vehicle-types`.
- `GET /api/vehicle-brands`.
- `GET /api/vehicle-models`.
- `GET /api/service-types`.
- `GET /api/payment-methods`.
- `GET /api/payment-statuses`.
- `GET /api/invoice-statuses`.

### Client

- `GET /api/client/dashboard`.
- `GET /api/client/my-vehicles`.
- `GET /api/client/my-service-orders`.
- `GET /api/client/my-invoices`.
- `GET /api/client/pending-approvals`.
- `POST /api/client/order-services/{orderServiceId:int}/approve`.
- `POST /api/client/order-services/{orderServiceId:int}/reject`.
- `POST /api/client/order-service-parts/{orderServicePartId:int}/approve`.
- `POST /api/client/order-service-parts/{orderServicePartId:int}/reject`.

### Receptionist

- `GET /api/receptionist/dashboard`.
- `POST /api/receptionist/create-client-with-vehicle`.
- `POST /api/workshop-intake/create-service-order`.
- `GET /api/search/clients`.
- `GET /api/search/vehicles`.
- `GET /api/search/service-orders`.
- `GET /api/search/invoices`.

### Mechanic

- `GET /api/mechanic/dashboard`.
- Endpoints de asignaciones mecanicas.
- Endpoints de flujo de ejecucion de servicio.
- Endpoints para ordenes activas y servicios asignados.
- Endpoints para solicitud de repuestos.

### Admin

- `GET /api/admin/dashboard`.
- `GET /api/admin/mechanics`.
- `GET /api/admin/mechanics/{personId:int}`.
- `GET /api/admin/mechanics/{personId:int}/workload`.
- `GET /api/admin/audits/recent`.
- `GET /api/admin/audits/by-entity`.
- `GET /api/admin/audits/by-user/{userId:int}`.
- `GET /api/admin/reports/sales`.
- `GET /api/admin/reports/inventory`.
- `GET /api/admin/reports/mechanics`.
- `GET /api/admin/reports/service-orders`.
- `GET /api/admin/reports/payments`.

### Ordenes de servicio

- `GET /api/service-orders`.
- `GET /api/service-orders/{id:int}`.
- `POST /api/service-orders`.
- `PUT /api/service-orders/{id:int}`.
- `DELETE /api/service-orders/{id:int}`.
- `GET /api/service-orders/{id:int}/full-detail`.
- `POST /api/service-orders/{id:int}/change-status`.
- `POST /api/service-orders/{id:int}/cancel`.
- `POST /api/service-orders/{id:int}/void`.
- `POST /api/service-orders/{id:int}/complete`.

### Inventario, compras, facturas y pagos

- Endpoints CRUD para `parts`, `part-categories`, `part-brands` y `suppliers`.
- Endpoints de negocio de inventario.
- Endpoints para compras de repuestos y detalles de compra.
- Endpoints para facturas, detalles de factura y estados de factura.
- Endpoints para pagos, metodos de pago, estados de pago y tarjetas.

## Validacion rapida

Comandos recomendados:

```powershell
dotnet restore
dotnet build .\AutoTallerManager.slnx
dotnet run --project Api\Api.csproj
```

Checklist:

- La solucion compila sin errores.
- La API inicia correctamente.
- La base de datos conecta.
- Las migraciones necesarias ya estan aplicadas.
- Los seeders se ejecutan sin duplicar datos.
- Swagger abre en entorno `Development`.
- El frontend puede consumir catalogos publicos y de taller.
- Los endpoints protegidos reciben y validan JWT.

## Relacion con el frontend

Frontend repository: https://github.com/ximenaa19/AutoTallerManager-Fronted

El frontend consume esta API para presentar las interfaces de:

- Admin.
- Mechanic.
- Receptionist.
- Client.

La API tiene CORS configurado para origenes locales comunes:

- `http://localhost:3000`.
- `http://localhost:5173`.
- `http://localhost:4200`.

Si el frontend corre en otro puerto u origen, se debe ajustar la politica CORS de forma controlada.

## Buenas practicas

- No subir secretos reales.
- Usar variables de entorno para credenciales, JWT y cadenas de conexion productivas.
- Ejecutar `dotnet build .\AutoTallerManager.slnx` antes de hacer push.
- Revisar migraciones antes de aplicarlas.
- No modificar seeders para crear datos operativos falsos.
- Mantener separacion por capas.
- Evitar reglas de negocio en controllers.
- Mantener DTOs y servicios en `Application`.
- Mantener persistencia en `Infrastructure`.
- Coordinar cambios de base de datos con el equipo.

## Troubleshooting

### Error de conexion a base de datos

Verificar:

- MySQL esta encendido.
- Host, puerto, base de datos, usuario y password son correctos.
- La cadena de conexion corresponde al entorno actual.
- Las credenciales reales no estan hardcodeadas en archivos versionados.

### Error de migraciones pendientes

Revisar migraciones disponibles:

```powershell
dotnet ef migrations list --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

Aplicar migraciones solo si corresponde:

```powershell
dotnet ef database update --project Infrastructure\Infrastructure.csproj --startup-project Api\Api.csproj --context Infrastructure.Persistence.AppDbContext
```

### Swagger no abre

Verificar:

- La API esta ejecutandose.
- El entorno es `Development`.
- La URL usada corresponde al puerto activo.
- No hay otro proceso usando el mismo puerto.

### Seeders no cargan

Verificar:

- `SeedData:Enabled` esta habilitado o el entorno es `Development`.
- La base de datos conecta.
- Las tablas existen.
- Las migraciones requeridas estan aplicadas.

### Puerto ocupado

Cambiar el puerto en `Api/Properties/launchSettings.json` o detener el proceso que este usando el puerto actual.

### Error SSL local con MySQL

En entornos locales puede aparecer un error de autenticacion SSL dependiendo de la configuracion del servidor MySQL y del cliente. Revisar la configuracion SSL local de la cadena de conexion y del servidor. No deshabilitar ni debilitar SSL en entornos productivos.

## Estado del proyecto

El backend esta preparado para soportar los flujos principales de los cuatro roles:

- Admin.
- Mechanic.
- Receptionist.
- Client.

El proyecto contiene controladores, servicios, persistencia, migraciones y seeders para operar los dominios centrales del taller. Algunos detalles funcionales especificos pueden depender del estado de la base de datos local y de que las migraciones hayan sido aplicadas correctamente.
