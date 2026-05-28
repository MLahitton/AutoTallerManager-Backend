using Application.Common.Results;
using Application.Features.PersonRoles.Dtos;
using Application.Features.PersonRoles.Requests;

namespace Application.Features.PersonRoles;

public interface IPersonRoleService
{
    Task<Result<IReadOnlyList<PersonRoleDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PersonRoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PersonRoleDto>> CreateAsync(CreatePersonRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result<PersonRoleDto>> UpdateAsync(int id, UpdatePersonRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
