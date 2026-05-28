using Application.Common.Results;
using Application.Features.PartBrands.Dtos;
using Application.Features.PartBrands.Requests;

namespace Application.Features.PartBrands;

public interface IPartBrandService
{
    Task<Result<IReadOnlyList<PartBrandDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PartBrandDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PartBrandDto>> CreateAsync(CreatePartBrandRequest request, CancellationToken cancellationToken = default);

    Task<Result<PartBrandDto>> UpdateAsync(int id, UpdatePartBrandRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
