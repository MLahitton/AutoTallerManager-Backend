using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PersonRoles.Dtos;
using Application.Features.PersonRoles.Errors;
using Application.Features.PersonRoles.Requests;
using Domain.Entities;

namespace Application.Features.PersonRoles;

public class PersonRoleService : IPersonRoleService
{
    private const string CreateAuditActionTypeName = "CREATE";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string DeleteAuditActionTypeName = "DELETE";
    private const string PersonRoleEntityName = "PersonRole";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public PersonRoleService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<IReadOnlyList<PersonRoleDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRoles = await personRoleRepository.GetAllAsync(cancellationToken);

        var personRoleDtos = personRoles
            .OrderBy(x => x.PersonRoleId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PersonRoleDto>>.Success(personRoleDtos);
    }

    public async Task<Result<PersonRoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRole = await personRoleRepository.GetByIdAsync(id, cancellationToken);

        if (personRole is null)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.NotFound);
        }

        return Result<PersonRoleDto>.Success(MapToDto(personRole));
    }

    public async Task<Result<PersonRoleDto>> CreateAsync(CreatePersonRoleRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var personId = request?.PersonId ?? 0;
        var roleId = request?.RoleId ?? 0;
        var isActive = request?.IsActive ?? false;

        var validationError = Validate(personId, roleId);
        if (validationError is not null)
        {
            return Result<PersonRoleDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.PersonNotFound);
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roleExists = await roleRepository.ExistsAsync(
            x => x.RoleId == roleId,
            cancellationToken);

        if (!roleExists)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.RoleNotFound);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var relationAlreadyExists = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == roleId,
            cancellationToken);

        if (relationAlreadyExists)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.RelationAlreadyExists);
        }

        var personRole = new PersonRole
        {
            PersonId = personId,
            RoleId = roleId,
            IsActive = isActive
        };

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await personRoleRepository.AddAsync(personRole, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                PersonRoleEntityName,
                personRole.PersonRoleId,
                $"Role {roleId} assigned to person {personId}.",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<PersonRoleDto>.Success(MapToDto(personRole));
        }, cancellationToken);
    }

    public async Task<Result<PersonRoleDto>> UpdateAsync(int id, UpdatePersonRoleRequest request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRole = await personRoleRepository.GetByIdAsync(id, cancellationToken);

        if (personRole is null)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.NotFound);
        }

        var personId = request?.PersonId ?? 0;
        var roleId = request?.RoleId ?? 0;
        var isActive = request?.IsActive ?? false;

        var validationError = Validate(personId, roleId);
        if (validationError is not null)
        {
            return Result<PersonRoleDto>.Failure(validationError);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var personExists = await personRepository.ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.PersonNotFound);
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var roleExists = await roleRepository.ExistsAsync(
            x => x.RoleId == roleId,
            cancellationToken);

        if (!roleExists)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.RoleNotFound);
        }

        var relationAlreadyExists = await personRoleRepository.ExistsAsync(
            x => x.PersonId == personId && x.RoleId == roleId && x.PersonRoleId != id,
            cancellationToken);

        if (relationAlreadyExists)
        {
            return Result<PersonRoleDto>.Failure(PersonRoleErrors.RelationAlreadyExists);
        }

        personRole.PersonId = personId;
        personRole.RoleId = roleId;
        personRole.IsActive = isActive;

        personRoleRepository.Update(personRole);

        await _auditLogger.LogAsync(
            currentUserId,
            UpdateAuditActionTypeName,
            PersonRoleEntityName,
            personRole.PersonRoleId,
            isActive
                ? $"Role {roleId} updated for person {personId}."
                : $"Role {roleId} deactivated from person {personId}.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonRoleDto>.Success(MapToDto(personRole));
    }

    public async Task<Result> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRole = await personRoleRepository.GetByIdAsync(id, cancellationToken);

        if (personRole is null)
        {
            return Result.Failure(PersonRoleErrors.NotFound);
        }

        personRoleRepository.Remove(personRole);

        await _auditLogger.LogAsync(
            currentUserId,
            DeleteAuditActionTypeName,
            PersonRoleEntityName,
            personRole.PersonRoleId,
            $"Role {personRole.RoleId} removed from person {personRole.PersonId}.",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PersonRoleDto MapToDto(PersonRole personRole)
    {
        return new PersonRoleDto
        {
            PersonRoleId = personRole.PersonRoleId,
            PersonId = personRole.PersonId,
            RoleId = personRole.RoleId,
            IsActive = personRole.IsActive
        };
    }

    private static Error? Validate(int personId, int roleId)
    {
        if (personId <= 0)
        {
            return PersonRoleErrors.PersonIdInvalid;
        }

        if (roleId <= 0)
        {
            return PersonRoleErrors.RoleIdInvalid;
        }

        return null;
    }
}
