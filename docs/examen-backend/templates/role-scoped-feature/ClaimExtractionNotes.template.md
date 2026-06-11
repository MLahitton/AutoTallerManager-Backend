# Notas: extracción de claims JWT

## Dónde se generan los claims

`Api/Security/JwtTokenGenerator.cs`

| Claim | Contenido |
|-------|-----------|
| `userId` | Id del usuario (`User.UserId`) |
| `personId` | Id de la persona (`Person.PersonId`) |
| `email` | Email del usuario |
| `ClaimTypes.Role` | Un claim por cada rol (Admin, Client, ...) |

## Cómo leerlos en el controller

```csharp
using System.Security.Claims;

var userIdClaim = User.FindFirstValue("userId");
var personIdClaim = User.FindFirstValue("personId");

var roles = User.FindAll(ClaimTypes.Role)
    .Select(x => x.Value)
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();
```

## Reglas de oro

1. **Nunca** uses `userId` o `personId` del body del request para autorizar.
2. Si el claim falta o no parsea a `int > 0` → `return Unauthorized()`.
3. Pasa `currentPersonId` / `currentUserId` como **parámetros del método del servicio**, no dentro del Request DTO.
4. El atributo `[Authorize(Roles = "...")]` valida el rol; el servicio valida **ownership** (pertenencia).

## Referencias por rol

| Rol | Controller ejemplo | Claim principal |
|-----|-------------------|-----------------|
| Client | `ClientVehiclesController` | `personId` |
| Mechanic | `MechanicWorkflowController` | `personId` + `userId` |
| Admin/Receptionist | `SuppliersController` (mutaciones) | `userId` para auditoría |

## Errores comunes en examen

- Exponer datos de otro cliente porque no filtraste por `personId`.
- Permitir que el mecánico edite un servicio no asignado.
- Olvidar `Unauthorized()` cuando el claim es inválido.
