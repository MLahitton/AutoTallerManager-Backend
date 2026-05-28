using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Roles.Dtos;
using Application.Features.Roles.Errors;
using Application.Features.Roles.Requests;
using Domain.Entities;

namespace Application.Features.Roles;

public class RoleService : IRoleService
{
    private const int RoleNameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var roles = await roleRepository.GetAllAsync(cancellationToken);

        var roleDtos = roles
            .OrderBy(x => x.RoleId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<RoleDto>>.Success(roleDtos);
    }

    public async Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var role = await roleRepository.GetByIdAsync(id, cancellationToken);

        if (role is null)
        {
            return Result<RoleDto>.Failure(RoleErrors.NotFound);
        }

        return Result<RoleDto>.Success(MapToDto(role));
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = NormalizeRoleName(request?.RoleName);
        var validationError = ValidateRoleName(normalizedRoleName);
        if (validationError is not null)
        {
            return Result<RoleDto>.Failure(validationError);
        }

        var roleRepository = _unitOfWork.Repository<Role>();
        var nameAlreadyExists = await roleRepository.ExistsAsync(
            x => x.RoleName == normalizedRoleName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<RoleDto>.Failure(RoleErrors.NameAlreadyExists);
        }

        var role = new Role
        {
            RoleName = normalizedRoleName
        };

        await roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RoleDto>.Success(MapToDto(role));
    }

    public async Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var role = await roleRepository.GetByIdAsync(id, cancellationToken);

        if (role is null)
        {
            return Result<RoleDto>.Failure(RoleErrors.NotFound);
        }

        var normalizedRoleName = NormalizeRoleName(request?.RoleName);
        var validationError = ValidateRoleName(normalizedRoleName);
        if (validationError is not null)
        {
            return Result<RoleDto>.Failure(validationError);
        }

        var nameAlreadyExists = await roleRepository.ExistsAsync(
            x => x.RoleName == normalizedRoleName && x.RoleId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<RoleDto>.Failure(RoleErrors.NameAlreadyExists);
        }

        role.RoleName = normalizedRoleName;

        roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RoleDto>.Success(MapToDto(role));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var role = await roleRepository.GetByIdAsync(id, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        var personRoleRepository = _unitOfWork.Repository<PersonRole>();
        var roleIsInUse = await personRoleRepository.ExistsAsync(
            x => x.RoleId == id,
            cancellationToken);

        if (roleIsInUse)
        {
            return Result.Failure(RoleErrors.RoleInUse);
        }

        roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName
        };
    }

    private static string NormalizeRoleName(string? roleName)
    {
        return (roleName ?? string.Empty).Trim();
    }

    private static Error? ValidateRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return RoleErrors.NameRequired;
        }

        if (roleName.Length > RoleNameMaxLength)
        {
            return RoleErrors.NameTooLong;
        }

        return null;
    }
}
