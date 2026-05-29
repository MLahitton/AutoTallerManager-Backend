using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PaymentCards.Dtos;
using Application.Features.PaymentCards.Errors;
using Application.Features.PaymentCards.Requests;
using Domain.Entities;

namespace Application.Features.PaymentCards;

public class PaymentCardService : IPaymentCardService
{
    private const int CardHolderMaxLength = 100;
    private const int AuthorizationCodeMaxLength = 100;
    private const string CardPaymentMethodName = "Card";

    private readonly IUnitOfWork _unitOfWork;

    public PaymentCardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PaymentCardDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var paymentCards = await paymentCardRepository.GetAllAsync(cancellationToken);

        var paymentCardDtos = paymentCards
            .OrderBy(x => x.PaymentCardId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PaymentCardDto>>.Success(paymentCardDtos);
    }

    public async Task<Result<PaymentCardDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var paymentCard = await paymentCardRepository.GetByIdAsync(id, cancellationToken);

        if (paymentCard is null)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.NotFound);
        }

        return Result<PaymentCardDto>.Success(MapToDto(paymentCard));
    }

    public async Task<Result<PaymentCardDto>> CreateAsync(CreatePaymentCardRequest request, CancellationToken cancellationToken = default)
    {
        var paymentId = request?.PaymentId ?? 0;
        var cardTypeId = request?.CardTypeId ?? 0;
        var lastFourDigits = NormalizeRequiredText(request?.LastFourDigits);
        var cardHolder = NormalizeRequiredText(request?.CardHolder);
        var authorizationCode = NormalizeOptionalText(request?.AuthorizationCode);

        var validationError = Validate(paymentId, cardTypeId, lastFourDigits, cardHolder, authorizationCode);
        if (validationError is not null)
        {
            return Result<PaymentCardDto>.Failure(validationError);
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment is null)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.PaymentNotFound);
        }

        var paymentUsesCardMethod = await PaymentUsesCardMethodAsync(payment, cancellationToken);
        if (!paymentUsesCardMethod)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.PaymentMethodIsNotCardConflict);
        }

        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var cardTypeExists = await cardTypeRepository.ExistsAsync(
            x => x.CardTypeId == cardTypeId,
            cancellationToken);

        if (!cardTypeExists)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.CardTypeNotFound);
        }

        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var paymentAlreadyHasCard = await paymentCardRepository.ExistsAsync(
            x => x.PaymentId == paymentId,
            cancellationToken);

        if (paymentAlreadyHasCard)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.PaymentAlreadyHasCardConflict);
        }

        var paymentCard = new PaymentCard
        {
            PaymentId = paymentId,
            CardTypeId = cardTypeId,
            LastFourDigits = lastFourDigits,
            CardHolder = cardHolder,
            AuthorizationCode = authorizationCode
        };

        await paymentCardRepository.AddAsync(paymentCard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentCardDto>.Success(MapToDto(paymentCard));
    }

    public async Task<Result<PaymentCardDto>> UpdateAsync(int id, UpdatePaymentCardRequest request, CancellationToken cancellationToken = default)
    {
        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var paymentCard = await paymentCardRepository.GetByIdAsync(id, cancellationToken);

        if (paymentCard is null)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.NotFound);
        }

        var paymentId = request?.PaymentId ?? 0;
        var cardTypeId = request?.CardTypeId ?? 0;
        var lastFourDigits = NormalizeRequiredText(request?.LastFourDigits);
        var cardHolder = NormalizeRequiredText(request?.CardHolder);
        var authorizationCode = NormalizeOptionalText(request?.AuthorizationCode);

        var validationError = Validate(paymentId, cardTypeId, lastFourDigits, cardHolder, authorizationCode);
        if (validationError is not null)
        {
            return Result<PaymentCardDto>.Failure(validationError);
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment is null)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.PaymentNotFound);
        }

        var paymentUsesCardMethod = await PaymentUsesCardMethodAsync(payment, cancellationToken);
        if (!paymentUsesCardMethod)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.PaymentMethodIsNotCardConflict);
        }

        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var cardTypeExists = await cardTypeRepository.ExistsAsync(
            x => x.CardTypeId == cardTypeId,
            cancellationToken);

        if (!cardTypeExists)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.CardTypeNotFound);
        }

        var paymentAlreadyHasAnotherCard = await paymentCardRepository.ExistsAsync(
            x => x.PaymentId == paymentId && x.PaymentCardId != id,
            cancellationToken);

        if (paymentAlreadyHasAnotherCard)
        {
            return Result<PaymentCardDto>.Failure(PaymentCardErrors.PaymentAlreadyHasCardConflict);
        }

        paymentCard.PaymentId = paymentId;
        paymentCard.CardTypeId = cardTypeId;
        paymentCard.LastFourDigits = lastFourDigits;
        paymentCard.CardHolder = cardHolder;
        paymentCard.AuthorizationCode = authorizationCode;

        paymentCardRepository.Update(paymentCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentCardDto>.Success(MapToDto(paymentCard));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var paymentCard = await paymentCardRepository.GetByIdAsync(id, cancellationToken);

        if (paymentCard is null)
        {
            return Result.Failure(PaymentCardErrors.NotFound);
        }

        paymentCardRepository.Remove(paymentCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<bool> PaymentUsesCardMethodAsync(Payment payment, CancellationToken cancellationToken)
    {
        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(payment.PaymentMethodId, cancellationToken);

        if (paymentMethod is null)
        {
            return false;
        }

        return paymentMethod.Name.Equals(CardPaymentMethodName, StringComparison.OrdinalIgnoreCase);
    }

    private static PaymentCardDto MapToDto(PaymentCard paymentCard)
    {
        return new PaymentCardDto
        {
            PaymentCardId = paymentCard.PaymentCardId,
            PaymentId = paymentCard.PaymentId,
            CardTypeId = paymentCard.CardTypeId,
            LastFourDigits = paymentCard.LastFourDigits,
            CardHolder = paymentCard.CardHolder,
            AuthorizationCode = paymentCard.AuthorizationCode
        };
    }

    private static Error? Validate(
        int paymentId,
        int cardTypeId,
        string lastFourDigits,
        string cardHolder,
        string? authorizationCode)
    {
        if (paymentId <= 0)
        {
            return PaymentCardErrors.PaymentIdInvalid;
        }

        if (cardTypeId <= 0)
        {
            return PaymentCardErrors.CardTypeIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(lastFourDigits))
        {
            return PaymentCardErrors.LastFourDigitsRequired;
        }

        if (!IsValidLastFourDigits(lastFourDigits))
        {
            return PaymentCardErrors.LastFourDigitsInvalid;
        }

        if (string.IsNullOrWhiteSpace(cardHolder))
        {
            return PaymentCardErrors.CardHolderRequired;
        }

        if (cardHolder.Length > CardHolderMaxLength)
        {
            return PaymentCardErrors.CardHolderTooLong;
        }

        if (authorizationCode is not null && authorizationCode.Length > AuthorizationCodeMaxLength)
        {
            return PaymentCardErrors.AuthorizationCodeTooLong;
        }

        return null;
    }

    private static bool IsValidLastFourDigits(string value)
    {
        return value.Length == 4 && value.All(char.IsDigit);
    }

    private static string NormalizeRequiredText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
