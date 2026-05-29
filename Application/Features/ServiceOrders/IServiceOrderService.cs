using Application.Common.Results;
using Application.Features.ServiceOrders.Dtos;
using Application.Features.ServiceOrders.Requests;

namespace Application.Features.ServiceOrders;

public interface IServiceOrderService
{
    Task<Result<IReadOnlyList<ServiceOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderDto>> CreateAsync(CreateServiceOrderRequest request, CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderDto>> UpdateAsync(int id, UpdateServiceOrderRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
