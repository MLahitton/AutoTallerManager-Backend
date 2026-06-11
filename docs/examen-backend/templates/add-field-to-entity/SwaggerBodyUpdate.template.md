# Actualizar cuerpos Swagger — campo nuevo

## Referencia: Vehicle con Plate

`POST /api/vehicles`

```json
{
  "modelId": 1,
  "vehicleTypeId": 1,
  "vin": "1HGBH41JXMN109186",
  "plate": "ABC123",
  "year": 2020,
  "color": "Rojo",
  "mileage": 50000,
  "isActive": true
}
```

Al agregar un campo, inclúyelo en POST y PUT.

## Ejemplo genérico

`POST /api/vehicles` (con campo hipotético `newField`)

```json
{
  "modelId": 1,
  "vehicleTypeId": 1,
  "vin": "1HGBH41JXMN109187",
  "plate": "XYZ789",
  "newField": "valor de prueba",
  "year": 2021,
  "mileage": 10000,
  "isActive": true
}
```

## Pruebas de validación

| Prueba | Valor | Esperado |
|--------|-------|----------|
| Campo requerido vacío | `""` | 400 |
| Duplicado | Mismo valor que otro registro activo | 409 |
| Formato inválido | Según regex del servicio | 400 |
| GET por id | — | Respuesta incluye el campo |

## PUT

`PUT /api/vehicles/{id}` — incluir el campo en el body junto con los demás.

## Verificación en BD

Opcional en examen: consultar tabla en MySQL y confirmar que la columna se persiste.
