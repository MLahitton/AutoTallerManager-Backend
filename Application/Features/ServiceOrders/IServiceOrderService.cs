using Application.Common.Results;
using Application.Features.ServiceOrders.Dtos;
using Application.Features.ServiceOrders.Requests;

namespace Application.Features.ServiceOrders;

public interface IServiceOrderService
{
    Task<Result<IReadOnlyList<ServiceOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderDto>> CreateAsync(
        CreateServiceOrderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<Result<ServiceOrderDto>> UpdateAsync(
        int id,
        UpdateServiceOrderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
