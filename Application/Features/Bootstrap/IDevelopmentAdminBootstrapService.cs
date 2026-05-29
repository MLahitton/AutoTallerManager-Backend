using Application.Common.Results;

namespace Application.Features.Bootstrap;

public interface IDevelopmentAdminBootstrapService
{
    Task<Result<BootstrapAdminResultDto>> EnsureBootstrapAdminAsync(
        BootstrapAdminSettings settings,
        CancellationToken cancellationToken = default);
}
