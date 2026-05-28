using Application.Common.Results;
using Application.Features.PartCategories.Dtos;
using Application.Features.PartCategories.Requests;

namespace Application.Features.PartCategories;

public interface IPartCategoryService
{
    Task<Result<IReadOnlyList<PartCategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PartCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PartCategoryDto>> CreateAsync(CreatePartCategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<PartCategoryDto>> UpdateAsync(int id, UpdatePartCategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
