using Application.Common.Results;
using Application.Features.Roles.Dtos;
using Application.Features.Roles.Requests;

namespace Application.Features.Roles;

public interface IRoleService
{
    Task<Result<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
