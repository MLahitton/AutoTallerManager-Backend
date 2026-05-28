using Application.Common.Results;
using Application.Features.PaymentMethods.Dtos;
using Application.Features.PaymentMethods.Requests;

namespace Application.Features.PaymentMethods;

public interface IPaymentMethodService
{
    Task<Result<IReadOnlyList<PaymentMethodDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PaymentMethodDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PaymentMethodDto>> CreateAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default);

    Task<Result<PaymentMethodDto>> UpdateAsync(int id, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
