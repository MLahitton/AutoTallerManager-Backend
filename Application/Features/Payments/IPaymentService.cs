using Application.Common.Results;
using Application.Features.Payments.Dtos;
using Application.Features.Payments.Requests;

namespace Application.Features.Payments;

public interface IPaymentService
{
    Task<Result<IReadOnlyList<PaymentDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PaymentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);

    Task<Result<PaymentDto>> UpdateAsync(int id, UpdatePaymentRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
