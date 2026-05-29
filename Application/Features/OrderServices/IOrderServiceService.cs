using Application.Common.Results;
using Application.Features.OrderServices.Dtos;
using Application.Features.OrderServices.Requests;

namespace Application.Features.OrderServices;

public interface IOrderServiceService
{
    Task<Result<IReadOnlyList<OrderServiceDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<OrderServiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<OrderServiceDto>> CreateAsync(CreateOrderServiceRequest request, CancellationToken cancellationToken = default);

    Task<Result<OrderServiceDto>> UpdateAsync(int id, UpdateOrderServiceRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
