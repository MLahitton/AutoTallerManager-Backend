# Pruebas Swagger por rol — feature scoped

## Cuentas demo (`DemoAccountsSeeder.cs`)

| Rol | Email | Password |
|-----|-------|----------|
| Receptionist | tmedina@gmail.com | Testing123* |
| Mechanic | nzabala1@gmail.com | Testing123* |
| Client | rolito1@gmail.com | Testing123* |

Admin: revisar `BootstrapAdmin` en `appsettings.Development.json`.

## Flujo de prueba

1. `POST /api/auth/login` con email del rol.
2. Copiar `accessToken` → **Authorize**.
3. Llamar endpoint scoped.
4. Repetir con otro rol para verificar denegación.

## Client

| Endpoint | Qué validar |
|----------|-------------|
| `GET /api/client/my-vehicles` | Solo vehículos del `personId` del token |
| `GET /api/client/pending-approvals` | Solo aprobaciones pendientes propias |
| `POST /api/client/order-services/{id}/approve` | 200 si es su orden; 409 si no |

**Prueba negativa:** usar token Client en `GET /api/admin/reports/sales` → 403.

## Mechanic

| Endpoint | Qué validar |
|----------|-------------|
| `GET /api/mechanic/my-assigned-services` | Lista filtrada por asignación |
| `PUT /api/mechanic/order-services/{id}/work-performed` | 200 si asignado; 409 si no |

Login: `nzabala1@gmail.com` / `Testing123*`

## Admin / Receptionist

| Endpoint | Rol |
|----------|-----|
| `GET /api/vehicles` | Admin, Receptionist |
| `POST /api/inventory/register-purchase` | Admin, Receptionist |
| `POST /api/inventory/purchases/{id}/cancel` | **Solo Admin** |

## Checklist

- [ ] Token del rol correcto.
- [ ] Caso éxito con datos propios/asignados.
- [ ] Caso 409 intentando recurso ajeno.
- [ ] Caso 401/403 con rol incorrecto.
