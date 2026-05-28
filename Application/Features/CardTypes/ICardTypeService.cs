using Application.Common.Results;
using Application.Features.CardTypes.Dtos;
using Application.Features.CardTypes.Requests;

namespace Application.Features.CardTypes;

public interface ICardTypeService
{
    Task<Result<IReadOnlyList<CardTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<CardTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CardTypeDto>> CreateAsync(CreateCardTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result<CardTypeDto>> UpdateAsync(int id, UpdateCardTypeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
