using Application.Common.Results;
using Application.Features.PaymentCards.Dtos;
using Application.Features.PaymentCards.Requests;

namespace Application.Features.PaymentCards;

public interface IPaymentCardService
{
    Task<Result<IReadOnlyList<PaymentCardDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PaymentCardDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PaymentCardDto>> CreateAsync(CreatePaymentCardRequest request, CancellationToken cancellationToken = default);

    Task<Result<PaymentCardDto>> UpdateAsync(int id, UpdatePaymentCardRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
