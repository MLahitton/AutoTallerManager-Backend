using Application.Common.Results;
using Application.Features.PaymentStatuses.Dtos;
using Application.Features.PaymentStatuses.Requests;

namespace Application.Features.PaymentStatuses;

public interface IPaymentStatusService
{
    Task<Result<IReadOnlyList<PaymentStatusDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PaymentStatusDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PaymentStatusDto>> CreateAsync(CreatePaymentStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result<PaymentStatusDto>> UpdateAsync(int id, UpdatePaymentStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
