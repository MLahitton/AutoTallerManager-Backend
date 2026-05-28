using Application.Common.Results;
using Application.Features.Users.Dtos;
using Application.Features.Users.Requests;

namespace Application.Features.Users;

public interface IUserService
{
    Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
