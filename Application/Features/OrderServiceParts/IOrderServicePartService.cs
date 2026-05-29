using Application.Common.Results;
using Application.Features.OrderServiceParts.Dtos;
using Application.Features.OrderServiceParts.Requests;

namespace Application.Features.OrderServiceParts;

public interface IOrderServicePartService
{
    Task<Result<IReadOnlyList<OrderServicePartDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<OrderServicePartDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<OrderServicePartDto>> CreateAsync(
        CreateOrderServicePartRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<OrderServicePartDto>> UpdateAsync(
        int id,
        UpdateOrderServicePartRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
