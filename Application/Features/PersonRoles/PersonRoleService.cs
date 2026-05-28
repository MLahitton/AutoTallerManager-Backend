using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PersonRoles.Dtos;
using Application.Features.PersonRoles.Errors;
using Application.Features.PersonRoles.Requests;
using Domain.Entities;

namespace Application.Features.PersonRoles;

public class PersonRoleService : IPersonRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public PersonRoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    public async Task<Result<PersonRoleDto>> CreateAsync(CreatePersonRoleRequest request, CancellationToken cancellationToken = default)
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

        await personRoleRepository.AddAsync(personRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonRoleDto>.Success(MapToDto(personRole));
    }

    public async Task<Result<PersonRoleDto>> UpdateAsync(int id, UpdatePersonRoleRequest request, CancellationToken cancellationToken = default)
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PersonRoleDto>.Success(MapToDto(personRole));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var personRole = await personRoleRepository.GetByIdAsync(id, cancellationToken);

        if (personRole is null)
        {
            return Result.Failure(PersonRoleErrors.NotFound);
        }

        personRoleRepository.Remove(personRole);
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
