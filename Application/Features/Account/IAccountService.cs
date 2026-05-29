using Application.Common.Results;
using Application.Features.Account.Dtos;
using Application.Features.Account.Requests;

namespace Application.Features.Account;

public interface IAccountService
{
    Task<Result<AccountProfileDto>> GetMeAsync(int userId, int personId, CancellationToken cancellationToken = default);

    Task<Result<AccountProfileDto>> UpdateMeAsync(int userId, int personId, UpdateAccountProfileRequest request, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
