using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Payments.Dtos;
using Application.Features.Payments.Errors;
using Application.Features.Payments.Requests;
using Domain.Entities;

namespace Application.Features.Payments;

public class PaymentService : IPaymentService
{
    private const int ReferenceMaxLength = 100;
    private const string CompletedStatusName = "Completed";
    private const string CardPaymentMethodName = "Card";

    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payments = await paymentRepository.GetAllAsync(cancellationToken);

        var paymentDtos = payments
            .OrderBy(x => x.PaymentId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PaymentDto>>.Success(paymentDtos);
    }

    public async Task<Result<PaymentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepository.GetByIdAsync(id, cancellationToken);

        if (payment is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.NotFound);
        }

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var invoiceId = request?.InvoiceId ?? 0;
        var paymentMethodId = request?.PaymentMethodId ?? 0;
        var paymentStatusId = request?.PaymentStatusId ?? 0;
        var paymentDate = request?.PaymentDate ?? DateTime.UtcNow;
        var amount = request?.Amount ?? 0m;
        var reference = NormalizeReference(request?.Reference);

        var validationError = Validate(invoiceId, paymentMethodId, paymentStatusId, paymentDate, amount, reference);
        if (validationError is not null)
        {
            return Result<PaymentDto>.Failure(validationError);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.InvoiceNotFound);
        }

        if (invoice.Total <= 0m)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.InvoiceTotalInvalid);
        }

        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(paymentMethodId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.PaymentMethodNotFound);
        }

        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatus = await paymentStatusRepository.GetByIdAsync(paymentStatusId, cancellationToken);

        if (paymentStatus is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.PaymentStatusNotFound);
        }

        if (IsCompletedStatus(paymentStatus.Name))
        {
            var completedStatusIds = await GetCompletedStatusIdsAsync(cancellationToken);
            var completedPaidAmount = await GetCompletedPaidAmountAsync(
                invoiceId,
                excludePaymentId: null,
                completedStatusIds,
                cancellationToken);

            if (completedPaidAmount + amount > invoice.Total)
            {
                return Result<PaymentDto>.Failure(PaymentErrors.CompletedPaymentsExceedInvoiceTotalConflict);
            }
        }

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            PaymentMethodId = paymentMethodId,
            PaymentStatusId = paymentStatusId,
            PaymentDate = paymentDate,
            Amount = amount,
            Reference = reference
        };

        var paymentRepository = _unitOfWork.Repository<Payment>();
        await paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> UpdateAsync(int id, UpdatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepository.GetByIdAsync(id, cancellationToken);

        if (payment is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.NotFound);
        }

        var invoiceId = request?.InvoiceId ?? 0;
        var paymentMethodId = request?.PaymentMethodId ?? 0;
        var paymentStatusId = request?.PaymentStatusId ?? 0;
        var paymentDate = request?.PaymentDate ?? default;
        var amount = request?.Amount ?? 0m;
        var reference = NormalizeReference(request?.Reference);

        var validationError = Validate(invoiceId, paymentMethodId, paymentStatusId, paymentDate, amount, reference);
        if (validationError is not null)
        {
            return Result<PaymentDto>.Failure(validationError);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.InvoiceNotFound);
        }

        if (invoice.Total <= 0m)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.InvoiceTotalInvalid);
        }

        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(paymentMethodId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.PaymentMethodNotFound);
        }

        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var paymentHasCardDetails = await paymentCardRepository.ExistsAsync(
            x => x.PaymentId == id,
            cancellationToken);

        if (paymentHasCardDetails &&
            !paymentMethod.Name.Equals(CardPaymentMethodName, StringComparison.OrdinalIgnoreCase))
        {
            return Result<PaymentDto>.Failure(PaymentErrors.PaymentMethodCannotChangeBecauseCardExistsConflict);
        }

        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatus = await paymentStatusRepository.GetByIdAsync(paymentStatusId, cancellationToken);

        if (paymentStatus is null)
        {
            return Result<PaymentDto>.Failure(PaymentErrors.PaymentStatusNotFound);
        }

        if (IsCompletedStatus(paymentStatus.Name))
        {
            var completedStatusIds = await GetCompletedStatusIdsAsync(cancellationToken);
            var completedPaidAmount = await GetCompletedPaidAmountAsync(
                invoiceId,
                excludePaymentId: id,
                completedStatusIds,
                cancellationToken);

            if (completedPaidAmount + amount > invoice.Total)
            {
                return Result<PaymentDto>.Failure(PaymentErrors.CompletedPaymentsExceedInvoiceTotalConflict);
            }
        }

        payment.InvoiceId = invoiceId;
        payment.PaymentMethodId = paymentMethodId;
        payment.PaymentStatusId = paymentStatusId;
        payment.PaymentDate = paymentDate;
        payment.Amount = amount;
        payment.Reference = reference;

        paymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepository.GetByIdAsync(id, cancellationToken);

        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound);
        }

        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var inUseByPaymentCard = await paymentCardRepository.ExistsAsync(
            x => x.PaymentId == id,
            cancellationToken);

        if (inUseByPaymentCard)
        {
            return Result.Failure(PaymentErrors.InUse);
        }

        paymentRepository.Remove(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<int[]> GetCompletedStatusIdsAsync(CancellationToken cancellationToken)
    {
        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatuses = await paymentStatusRepository.GetAllAsync(cancellationToken);

        return paymentStatuses
            .Where(x => IsCompletedStatus(x.Name))
            .Select(x => x.PaymentStatusId)
            .Distinct()
            .ToArray();
    }

    private async Task<decimal> GetCompletedPaidAmountAsync(
        int invoiceId,
        int? excludePaymentId,
        int[] completedStatusIds,
        CancellationToken cancellationToken)
    {
        if (completedStatusIds.Length == 0)
        {
            return 0m;
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        IReadOnlyList<Payment> completedPayments;

        if (excludePaymentId.HasValue)
        {
            completedPayments = await paymentRepository.FindAsync(
                x => x.InvoiceId == invoiceId &&
                     completedStatusIds.Contains(x.PaymentStatusId) &&
                     x.PaymentId != excludePaymentId.Value,
                cancellationToken);
        }
        else
        {
            completedPayments = await paymentRepository.FindAsync(
                x => x.InvoiceId == invoiceId &&
                     completedStatusIds.Contains(x.PaymentStatusId),
                cancellationToken);
        }

        return completedPayments.Sum(x => x.Amount);
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            PaymentId = payment.PaymentId,
            InvoiceId = payment.InvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            PaymentStatusId = payment.PaymentStatusId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            Reference = payment.Reference
        };
    }

    private static Error? Validate(
        int invoiceId,
        int paymentMethodId,
        int paymentStatusId,
        DateTime paymentDate,
        decimal amount,
        string? reference)
    {
        if (invoiceId <= 0)
        {
            return PaymentErrors.InvoiceIdInvalid;
        }

        if (paymentMethodId <= 0)
        {
            return PaymentErrors.PaymentMethodIdInvalid;
        }

        if (paymentStatusId <= 0)
        {
            return PaymentErrors.PaymentStatusIdInvalid;
        }

        if (paymentDate == default)
        {
            return PaymentErrors.PaymentDateInvalid;
        }

        if (amount <= 0m)
        {
            return PaymentErrors.AmountInvalid;
        }

        if (reference is not null && reference.Length > ReferenceMaxLength)
        {
            return PaymentErrors.ReferenceTooLong;
        }

        return null;
    }

    private static bool IsCompletedStatus(string paymentStatusName)
    {
        return paymentStatusName.Equals(CompletedStatusName, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeReference(string? reference)
    {
        var normalized = (reference ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
