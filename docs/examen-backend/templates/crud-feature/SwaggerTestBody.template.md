# Cuerpos de prueba Swagger — CRUD NewEntity

Base URL: `http://localhost:5077/swagger`

## 1. Login (Admin o Receptionist)

`POST /api/auth/login`

```json
{
  "email": "tmedina@gmail.com",
  "password": "Testing123*"
}
```

Copiar `accessToken` → Authorize en Swagger.

## 2. POST — Crear

`POST /api/new-entities`

```json
{
  "name": "Ejemplo Catálogo",
  "isActive": true
}
```

**Esperado:** 201 Created con `newEntityId` en la respuesta.

## 3. GET — Lista

`GET /api/new-entities`

**Esperado:** 200 OK, arreglo con el registro creado.

## 4. GET — Por id

`GET /api/new-entities/{id}`

**Esperado:** 200 OK con el DTO.

Probar con `id=999999` → **404** `NewEntities.NotFound`.

## 5. PUT — Actualizar

`PUT /api/new-entities/{id}`

```json
{
  "name": "Ejemplo Actualizado",
  "isActive": false
}
```

**Esperado:** 200 OK.

## 6. DELETE

`DELETE /api/new-entities/{id}`

**Esperado:** 204 No Content.

## 7. Validaciones a probar

| Prueba | Body | HTTP esperado |
|--------|------|---------------|
| Nombre vacío | `{ "name": "", "isActive": true }` | 400 |
| Nombre duplicado | Mismo name que registro existente | 409 |
| Sin token | No autorizar | 401 |

## Referencia real

Probar el mismo flujo con `api/genders` o `api/suppliers` antes del examen para familiarizarte.
