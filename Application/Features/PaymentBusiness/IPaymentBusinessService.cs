using Application.Common.Results;
using Application.Features.PaymentBusiness.Dtos;
using Application.Features.PaymentBusiness.Requests;

namespace Application.Features.PaymentBusiness;

public interface IPaymentBusinessService
{
    Task<Result<RecordedPaymentDto>> RecordPaymentAsync(int invoiceId, RecordPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentSummaryDto>> GetPaymentSummaryAsync(int invoiceId, int currentPersonId, IReadOnlyList<string> currentRoles, CancellationToken cancellationToken = default);
    Task<Result<RecordedPaymentDto>> RefundAsync(int paymentId, CancellationToken cancellationToken = default);
}
