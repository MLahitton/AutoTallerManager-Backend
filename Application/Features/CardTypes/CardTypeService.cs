using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.CardTypes.Dtos;
using Application.Features.CardTypes.Errors;
using Application.Features.CardTypes.Requests;
using Domain.Entities;

namespace Application.Features.CardTypes;

public class CardTypeService : ICardTypeService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public CardTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CardTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var cardTypes = await cardTypeRepository.GetAllAsync(cancellationToken);

        var cardTypeDtos = cardTypes
            .OrderBy(x => x.CardTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<CardTypeDto>>.Success(cardTypeDtos);
    }

    public async Task<Result<CardTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var cardType = await cardTypeRepository.GetByIdAsync(id, cancellationToken);

        if (cardType is null)
        {
            return Result<CardTypeDto>.Failure(CardTypeErrors.NotFound);
        }

        return Result<CardTypeDto>.Success(MapToDto(cardType));
    }

    public async Task<Result<CardTypeDto>> CreateAsync(
        CreateCardTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<CardTypeDto>.Failure(validationError);
        }

        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var nameAlreadyExists = await cardTypeRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<CardTypeDto>.Failure(CardTypeErrors.NameAlreadyExists);
        }

        var cardType = new CardType
        {
            Name = normalizedName
        };

        await cardTypeRepository.AddAsync(cardType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CardTypeDto>.Success(MapToDto(cardType));
    }

    public async Task<Result<CardTypeDto>> UpdateAsync(
        int id,
        UpdateCardTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var cardType = await cardTypeRepository.GetByIdAsync(id, cancellationToken);

        if (cardType is null)
        {
            return Result<CardTypeDto>.Failure(CardTypeErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<CardTypeDto>.Failure(validationError);
        }

        var nameAlreadyExists = await cardTypeRepository.ExistsAsync(
            x => x.Name == normalizedName && x.CardTypeId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<CardTypeDto>.Failure(CardTypeErrors.NameAlreadyExists);
        }

        cardType.Name = normalizedName;

        cardTypeRepository.Update(cardType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CardTypeDto>.Success(MapToDto(cardType));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cardTypeRepository = _unitOfWork.Repository<CardType>();
        var cardType = await cardTypeRepository.GetByIdAsync(id, cancellationToken);

        if (cardType is null)
        {
            return Result.Failure(CardTypeErrors.NotFound);
        }

        var paymentCardRepository = _unitOfWork.Repository<PaymentCard>();
        var inUse = await paymentCardRepository.ExistsAsync(
            x => x.CardTypeId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(CardTypeErrors.InUse);
        }

        cardTypeRepository.Remove(cardType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static CardTypeDto MapToDto(CardType cardType)
    {
        return new CardTypeDto
        {
            CardTypeId = cardType.CardTypeId,
            Name = cardType.Name
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CardTypeErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return CardTypeErrors.NameTooLong;
        }

        return null;
    }
}
