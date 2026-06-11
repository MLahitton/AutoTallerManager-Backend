# Cuerpos de prueba Swagger — Acción de negocio

Base URL: `http://localhost:5077/swagger`

## Preparación

1. Login con el rol correcto (ej. Admin para cancelar compra).
2. Tener datos previos (compra registrada, factura, orden de servicio, etc.).

## Ejemplo: ejecutar acción con body

`POST /api/new-feature/entities/{newEntityId}/execute-action`

```json
{
  "reason": "Error en la orden de compra"
}
```

**Esperado éxito:** 200 OK con `NewFeatureActionResultDto`.

## Casos de error a probar

| Caso | Cómo | HTTP |
|------|------|------|
| Sin motivo | `"reason": ""` o omitir | 400 |
| Id inexistente | `newEntityId=999999` | 404 |
| Ya procesado | Repetir la misma acción | 409 |
| Rol incorrecto | Token Client en endpoint Admin | 401/403 |
| Sin token | No autorizar | 401 |

## Referencias reales para practicar

### Cancelar compra (Admin)

`POST /api/inventory/purchases/{purchaseId}/cancel`

```json
{
  "reason": "Compra duplicada por error"
}
```

### Aprobar servicio (Client)

`POST /api/client/order-services/{orderServiceId}/approve`

(Sin body — solo token de cliente con `personId` correcto.)

### Registrar pago

Ver `PaymentBusinessController` y body de `RecordPaymentRequest` en el proyecto.

## Checklist post-acción

- [ ] Respuesta 200 con DTO esperado.
- [ ] Estado en BD actualizado (tabla principal y relacionadas).
- [ ] Si aplica auditoría: `GET /api/admin/audits/by-entity`.
