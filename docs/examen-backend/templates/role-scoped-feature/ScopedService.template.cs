// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewFeature/NewScopedService.cs
// Referencia Client: Application/Features/ClientApprovals/ClientApprovalService.cs
// Referencia Mechanic: Application/Features/ServiceExecution/ServiceExecutionService.cs
// Referencia Admin agregado: Application/Features/AdminMechanics/AdminMechanicsService.cs

using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.NewFeature.Dtos;
using Application.Features.NewFeature.Errors;
using Domain.Entities;

namespace Application.Features.NewFeature;

public class NewScopedService : INewScopedService
{
    private const string ClientRoleName = "Client";
    private const string MechanicRoleName = "Mechanic";

    private readonly IUnitOfWork _unitOfWork;

    public NewScopedService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<NewScopedDto>>> GetMyNewEntitiesAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        if (currentPersonId <= 0)
        {
            return Result<IReadOnlyList<NewScopedDto>>.Failure(NewScopedErrors.CurrentPersonIdInvalid);
        }

        // Validar que la persona tiene el rol esperado (patrón ClientApprovalService)
        var roleError = await ValidatePersonHasRoleAsync(currentPersonId, ClientRoleName, cancellationToken);
        if (roleError is not null)
        {
            return Result<IReadOnlyList<NewScopedDto>>.Failure(roleError);
        }

        // Filtrar SOLO registros del currentPersonId (ownership / asignación)
        // Ejemplo Client: filtrar por VehicleOwnerHistory
        // Ejemplo Mechanic: filtrar por MechanicAssignment donde MechanicPersonId == currentPersonId

        var items = await _unitOfWork.Repository<NewEntity>().FindAsync(
            x => x.OwnerPersonId == currentPersonId,  // Ajustar propiedad real
            cancellationToken);

        var dtos = items
            .OrderByDescending(x => x.NewEntityId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<NewScopedDto>>.Success(dtos);
    }

    public async Task<Result<NewScopedDto>> ConfirmAsync(
        int newEntityId,
        int currentPersonId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (newEntityId <= 0)
        {
            return Result<NewScopedDto>.Failure(NewScopedErrors.NewEntityIdInvalid);
        }

        var entity = await _unitOfWork.Repository<NewEntity>().GetByIdAsync(newEntityId, cancellationToken);
        if (entity is null)
        {
            return Result<NewScopedDto>.Failure(NewScopedErrors.NewEntityNotFound);
        }

        // Ownership: el registro debe pertenecer al currentPersonId
        if (entity.OwnerPersonId != currentPersonId)
        {
            return Result<NewScopedDto>.Failure(NewScopedErrors.AccessDeniedConflict);
        }

        // Regla de negocio adicional...
        // entity.IsConfirmed = true;
        // _unitOfWork.Repository<NewEntity>().Update(entity);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NewScopedDto>.Success(MapToDto(entity));
    }

    private async Task<Error?> ValidatePersonHasRoleAsync(
        int personId,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = (await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken))
            .FirstOrDefault(x => x.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            return NewScopedErrors.RoleNotFound;
        }

        var hasRole = await _unitOfWork.Repository<PersonRole>().ExistsAsync(
            x => x.PersonId == personId && x.RoleId == role.RoleId && x.IsActive,
            cancellationToken);

        if (!hasRole)
        {
            return NewScopedErrors.PersonDoesNotHaveRoleInvalid;
        }

        return null;
    }

    private static NewScopedDto MapToDto(NewEntity entity)
    {
        return new NewScopedDto
        {
            NewEntityId = entity.NewEntityId,
            Name = entity.Name
        };
    }
}

// Errores sugeridos en NewScopedErrors.cs:
// CurrentPersonIdInvalid, NewEntityNotFound, AccessDeniedConflict (sufijo Conflict → 409)
// Referencia: ServiceExecutionErrors.ClientCannotAccessServiceOrderConflict
//             ServiceExecutionErrors.MechanicNotAssignedConflict
