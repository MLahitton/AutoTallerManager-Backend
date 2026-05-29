using Application.Common.Results;
using Application.Features.PartPurchases.Dtos;
using Application.Features.PartPurchases.Requests;

namespace Application.Features.PartPurchases;

public interface IPartPurchaseService
{
    Task<Result<IReadOnlyList<PartPurchaseDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PartPurchaseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PartPurchaseDto>> CreateAsync(CreatePartPurchaseRequest request, CancellationToken cancellationToken = default);

    Task<Result<PartPurchaseDto>> UpdateAsync(int id, UpdatePartPurchaseRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
