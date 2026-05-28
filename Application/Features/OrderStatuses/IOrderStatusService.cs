using Application.Common.Results;
using Application.Features.OrderStatuses.Dtos;
using Application.Features.OrderStatuses.Requests;

namespace Application.Features.OrderStatuses;

public interface IOrderStatusService
{
    Task<Result<IReadOnlyList<OrderStatusDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<OrderStatusDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<OrderStatusDto>> CreateAsync(CreateOrderStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result<OrderStatusDto>> UpdateAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
