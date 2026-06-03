using Application.Common.Auditing;
using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PaymentBusiness.Dtos;
using Application.Features.PaymentBusiness.Errors;
using Application.Features.PaymentBusiness.Requests;
using Domain.Entities;

namespace Application.Features.PaymentBusiness;

public class PaymentBusinessService : IPaymentBusinessService
{
    private const int ReferenceMaxLength = 100;
    private const int CardHolderMaxLength = 100;
    private const int AuthorizationCodeMaxLength = 100;
    private const string CompletedStatusName = "Completed";
    private const string RefundedStatusName = "Refunded";
    private const string CardPaymentMethodName = "Card";
    private const string ClientRoleName = "Client";
    private const string CreateAuditActionTypeName = "CREATE";
    private const string UpdateAuditActionTypeName = "UPDATE";
    private const string PaymentEntityName = "Payment";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogger _auditLogger;

    public PaymentBusinessService(IUnitOfWork unitOfWork, IAuditLogger auditLogger)
    {
        _unitOfWork = unitOfWork;
        _auditLogger = auditLogger;
    }

    public async Task<Result<RecordedPaymentDto>> RecordPaymentAsync(
        int invoiceId,
        RecordPaymentRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.InvoiceIdInvalid);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.InvoiceNotFound);
        }

        if (invoice.Total <= 0m)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.InvoiceTotalInvalid);
        }

        var paymentMethodId = request?.PaymentMethodId ?? 0;
        if (paymentMethodId <= 0)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.PaymentMethodIdInvalid);
        }

        var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetByIdAsync(paymentMethodId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.PaymentMethodNotFound);
        }

        var paymentDate = request?.PaymentDate ?? DateTime.UtcNow;
        if (paymentDate == default)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.PaymentDateInvalid);
        }

        var amount = request?.Amount ?? 0m;
        if (amount <= 0m)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.AmountInvalid);
        }

        var reference = NormalizeOptionalText(request?.Reference);
        if (reference is not null && reference.Length > ReferenceMaxLength)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.ReferenceTooLong);
        }

        var paymentStatus = await ResolvePaymentStatusAsync(request?.PaymentStatusId, cancellationToken);
        if (paymentStatus.Error is not null)
        {
            return Result<RecordedPaymentDto>.Failure(paymentStatus.Error);
        }

        if (IsStatusName(paymentStatus.Value!.Name, CompletedStatusName))
        {
            var completedStatusIds = await GetPaymentStatusIdsByNameAsync(CompletedStatusName, cancellationToken);
            var completedPaidAmount = await GetPaidAmountByStatusIdsAsync(
                invoiceId,
                completedStatusIds,
                excludePaymentId: null,
                cancellationToken);

            if (completedPaidAmount + amount > invoice.Total)
            {
                return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.CompletedPaymentsExceedInvoiceTotalConflict);
            }
        }

        var isCardPaymentMethod = IsStatusName(paymentMethod.Name, CardPaymentMethodName);
        var cardValidationResult = await ValidateCardInformationAsync(
            isCardPaymentMethod,
            request?.Card,
            cancellationToken);

        if (cardValidationResult.Error is not null)
        {
            return Result<RecordedPaymentDto>.Failure(cardValidationResult.Error);
        }

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            PaymentMethodId = paymentMethod.PaymentMethodId,
            PaymentStatusId = paymentStatus.Value!.PaymentStatusId,
            PaymentDate = paymentDate,
            Amount = amount,
            Reference = reference
        };

        var paymentRepository = _unitOfWork.Repository<Payment>();
        await paymentRepository.AddAsync(payment, cancellationToken);

        PaymentCard? paymentCard = null;
        if (isCardPaymentMethod)
        {
            var card = cardValidationResult.Value!;
            paymentCard = new PaymentCard
            {
                Payment = payment,
                CardTypeId = card.CardTypeId,
                LastFourDigits = card.LastFourDigits!,
                CardHolder = card.CardHolder!,
                AuthorizationCode = card.AuthorizationCode
            };

            await _unitOfWork.Repository<PaymentCard>().AddAsync(paymentCard, cancellationToken);
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await _auditLogger.LogAsync(
                currentUserId,
                CreateAuditActionTypeName,
                PaymentEntityName,
                payment.PaymentId,
                $"Payment {payment.PaymentId} recorded for invoice {invoiceId}. Amount: {amount}.",
                transactionCancellationToken);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return Result<RecordedPaymentDto>.Success(MapRecordedPayment(payment, paymentCard?.PaymentCardId));
        }, cancellationToken);
    }

    public async Task<Result<PaymentSummaryDto>> GetPaymentSummaryAsync(
        int invoiceId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return Result<PaymentSummaryDto>.Failure(PaymentBusinessErrors.InvoiceIdInvalid);
        }

        var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result<PaymentSummaryDto>.Failure(PaymentBusinessErrors.InvoiceNotFound);
        }

        if (HasRole(currentRoles, ClientRoleName))
        {
            var canAccess = await CanClientAccessInvoiceAsync(invoice, currentPersonId, cancellationToken);
            if (!canAccess)
            {
                return Result<PaymentSummaryDto>.Failure(PaymentBusinessErrors.ClientCannotAccessInvoiceConflict);
            }
        }

        var paymentStatuses = await _unitOfWork.Repository<PaymentStatus>().GetAllAsync(cancellationToken);
        var completedStatusIds = paymentStatuses
            .Where(x => IsStatusName(x.Name, CompletedStatusName))
            .Select(x => x.PaymentStatusId)
            .Distinct()
            .ToArray();

        if (completedStatusIds.Length == 0)
        {
            return Result<PaymentSummaryDto>.Failure(PaymentBusinessErrors.CompletedStatusNotFound);
        }

        var refundedStatusIds = paymentStatuses
            .Where(x => IsStatusName(x.Name, RefundedStatusName))
            .Select(x => x.PaymentStatusId)
            .Distinct()
            .ToArray();

        if (refundedStatusIds.Length == 0)
        {
            return Result<PaymentSummaryDto>.Failure(PaymentBusinessErrors.RefundedStatusNotFound);
        }

        var payments = await _unitOfWork.Repository<Payment>().FindAsync(
            x => x.InvoiceId == invoiceId,
            cancellationToken);

        var completedPaidAmount = payments
            .Where(x => completedStatusIds.Contains(x.PaymentStatusId))
            .Sum(x => x.Amount);

        var refundedAmount = payments
            .Where(x => refundedStatusIds.Contains(x.PaymentStatusId))
            .Sum(x => x.Amount);

        var pendingAmount = invoice.Total - completedPaidAmount;
        if (pendingAmount < 0m)
        {
            pendingAmount = 0m;
        }

        var summary = new PaymentSummaryDto
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceTotal = invoice.Total,
            CompletedPaidAmount = completedPaidAmount,
            RefundedAmount = refundedAmount,
            PendingAmount = pendingAmount,
            Payments = payments
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.PaymentId)
                .Select(MapSummaryItem)
                .ToList()
        };

        return Result<PaymentSummaryDto>.Success(summary);
    }

    public async Task<Result<RecordedPaymentDto>> RefundAsync(
        int paymentId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (paymentId <= 0)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.PaymentIdInvalid);
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.PaymentNotFound);
        }

        var refundedStatusIds = await GetPaymentStatusIdsByNameAsync(RefundedStatusName, cancellationToken);
        if (refundedStatusIds.Length == 0)
        {
            return Result<RecordedPaymentDto>.Failure(PaymentBusinessErrors.RefundedStatusNotFound);
        }

        var refundedStatusId = refundedStatusIds[0];
        if (payment.PaymentStatusId != refundedStatusId)
        {
            payment.PaymentStatusId = refundedStatusId;
            paymentRepository.Update(payment);

            await _auditLogger.LogAsync(
                currentUserId,
                UpdateAuditActionTypeName,
                PaymentEntityName,
                payment.PaymentId,
                $"Payment {payment.PaymentId} refunded. Amount: {payment.Amount}.",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var paymentCard = (await _unitOfWork.Repository<PaymentCard>().FindAsync(
            x => x.PaymentId == paymentId,
            cancellationToken)).FirstOrDefault();

        return Result<RecordedPaymentDto>.Success(MapRecordedPayment(payment, paymentCard?.PaymentCardId));
    }

    private async Task<(PaymentStatus? Value, Error? Error)> ResolvePaymentStatusAsync(
        int? paymentStatusId,
        CancellationToken cancellationToken)
    {
        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();

        if (paymentStatusId.HasValue)
        {
            if (paymentStatusId.Value <= 0)
            {
                return (null, PaymentBusinessErrors.PaymentStatusIdInvalid);
            }

            var status = await paymentStatusRepository.GetByIdAsync(paymentStatusId.Value, cancellationToken);
            return status is null
                ? (null, PaymentBusinessErrors.PaymentStatusNotFound)
                : (status, null);
        }

        var allStatuses = await paymentStatusRepository.GetAllAsync(cancellationToken);
        var completedStatus = allStatuses.FirstOrDefault(x => IsStatusName(x.Name, CompletedStatusName));
        return completedStatus is null
            ? (null, PaymentBusinessErrors.CompletedStatusNotFound)
            : (completedStatus, null);
    }

    private async Task<(ValidatedCardDetails? Value, Error? Error)> ValidateCardInformationAsync(
        bool isCardPaymentMethod,
        PaymentCardDetailsRequest? card,
        CancellationToken cancellationToken)
    {
        if (!isCardPaymentMethod)
        {
            return card is null
                ? (null, null)
                : (null, PaymentBusinessErrors.CardDetailsNotAllowedInvalid);
        }

        if (card is null)
        {
            return (null, PaymentBusinessErrors.CardDetailsRequired);
        }

        if (card.CardTypeId <= 0)
        {
            return (null, PaymentBusinessErrors.CardTypeIdInvalid);
        }

        var cardTypeExists = await _unitOfWork.Repository<CardType>().ExistsAsync(
            x => x.CardTypeId == card.CardTypeId,
            cancellationToken);
        if (!cardTypeExists)
        {
            return (null, PaymentBusinessErrors.CardTypeNotFound);
        }

        var lastFourDigits = NormalizeOptionalText(card.LastFourDigits);
        if (string.IsNullOrWhiteSpace(lastFourDigits))
        {
            return (null, PaymentBusinessErrors.LastFourDigitsRequired);
        }

        if (lastFourDigits.Length != 4 || lastFourDigits.Any(x => !char.IsDigit(x)))
        {
            return (null, PaymentBusinessErrors.LastFourDigitsInvalid);
        }

        var cardHolder = NormalizeOptionalText(card.CardHolder);
        if (string.IsNullOrWhiteSpace(cardHolder))
        {
            return (null, PaymentBusinessErrors.CardHolderRequired);
        }

        if (cardHolder.Length > CardHolderMaxLength)
        {
            return (null, PaymentBusinessErrors.CardHolderTooLong);
        }

        var authorizationCode = NormalizeOptionalText(card.AuthorizationCode);
        if (authorizationCode is not null && authorizationCode.Length > AuthorizationCodeMaxLength)
        {
            return (null, PaymentBusinessErrors.AuthorizationCodeTooLong);
        }

        return (new ValidatedCardDetails
        {
            CardTypeId = card.CardTypeId,
            LastFourDigits = lastFourDigits,
            CardHolder = cardHolder,
            AuthorizationCode = authorizationCode
        }, null);
    }

    private async Task<bool> CanClientAccessInvoiceAsync(
        Invoice invoice,
        int currentPersonId,
        CancellationToken cancellationToken)
    {
        if (currentPersonId <= 0)
        {
            return false;
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(invoice.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return false;
        }

        return await _unitOfWork.Repository<VehicleOwnerHistory>().ExistsAsync(
            x => x.VehicleId == serviceOrder.VehicleId &&
                 x.PersonId == currentPersonId &&
                 x.EndDate == null,
            cancellationToken);
    }

    private async Task<int[]> GetPaymentStatusIdsByNameAsync(string statusName, CancellationToken cancellationToken)
    {
        var statuses = await _unitOfWork.Repository<PaymentStatus>().GetAllAsync(cancellationToken);

        return statuses
            .Where(x => IsStatusName(x.Name, statusName))
            .Select(x => x.PaymentStatusId)
            .Distinct()
            .ToArray();
    }

    private async Task<decimal> GetPaidAmountByStatusIdsAsync(
        int invoiceId,
        int[] statusIds,
        int? excludePaymentId,
        CancellationToken cancellationToken)
    {
        if (statusIds.Length == 0)
        {
            return 0m;
        }

        var payments = await _unitOfWork.Repository<Payment>().FindAsync(
            x => x.InvoiceId == invoiceId &&
                 statusIds.Contains(x.PaymentStatusId) &&
                 (!excludePaymentId.HasValue || x.PaymentId != excludePaymentId.Value),
            cancellationToken);

        return payments.Sum(x => x.Amount);
    }

    private static RecordedPaymentDto MapRecordedPayment(Payment payment, int? paymentCardId)
    {
        return new RecordedPaymentDto
        {
            PaymentId = payment.PaymentId,
            InvoiceId = payment.InvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            PaymentStatusId = payment.PaymentStatusId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            Reference = payment.Reference,
            PaymentCardId = paymentCardId
        };
    }

    private static PaymentSummaryItemDto MapSummaryItem(Payment payment)
    {
        return new PaymentSummaryItemDto
        {
            PaymentId = payment.PaymentId,
            PaymentMethodId = payment.PaymentMethodId,
            PaymentStatusId = payment.PaymentStatusId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            Reference = payment.Reference
        };
    }

    private static bool HasRole(IReadOnlyList<string> roles, string roleName)
    {
        return roles.Any(x => x.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsStatusName(string? value, string expected)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private sealed class ValidatedCardDetails
    {
        public int CardTypeId { get; set; }
        public string? LastFourDigits { get; set; }
        public string? CardHolder { get; set; }
        public string? AuthorizationCode { get; set; }
    }
}
