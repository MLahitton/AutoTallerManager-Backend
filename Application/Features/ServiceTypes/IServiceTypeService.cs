using Application.Common.Results;
using Application.Features.ServiceTypes.Dtos;
using Application.Features.ServiceTypes.Requests;

namespace Application.Features.ServiceTypes;

public interface IServiceTypeService
{
    Task<Result<IReadOnlyList<ServiceTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<ServiceTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ServiceTypeDto>> CreateAsync(CreateServiceTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result<ServiceTypeDto>> UpdateAsync(int id, UpdateServiceTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
