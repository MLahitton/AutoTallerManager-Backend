using Application.Common.Results;
using Application.Features.OrderStatusHistories.Dtos;
using Application.Features.OrderStatusHistories.Requests;

namespace Application.Features.OrderStatusHistories;

public interface IOrderStatusHistoryService
{
    Task<Result<IReadOnlyList<OrderStatusHistoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<OrderStatusHistoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<OrderStatusHistoryDto>> CreateAsync(CreateOrderStatusHistoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<OrderStatusHistoryDto>> UpdateAsync(int id, UpdateOrderStatusHistoryRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
