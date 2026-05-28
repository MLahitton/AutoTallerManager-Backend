using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PartCategories.Dtos;
using Application.Features.PartCategories.Errors;
using Application.Features.PartCategories.Requests;
using Domain.Entities;

namespace Application.Features.PartCategories;

public class PartCategoryService : IPartCategoryService
{
    private const int NameMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public PartCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PartCategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var partCategories = await partCategoryRepository.GetAllAsync(cancellationToken);

        var partCategoryDtos = partCategories
            .OrderBy(x => x.PartCategoryId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PartCategoryDto>>.Success(partCategoryDtos);
    }

    public async Task<Result<PartCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var partCategory = await partCategoryRepository.GetByIdAsync(id, cancellationToken);

        if (partCategory is null)
        {
            return Result<PartCategoryDto>.Failure(PartCategoryErrors.NotFound);
        }

        return Result<PartCategoryDto>.Success(MapToDto(partCategory));
    }

    public async Task<Result<PartCategoryDto>> CreateAsync(
        CreatePartCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PartCategoryDto>.Failure(validationError);
        }

        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var nameAlreadyExists = await partCategoryRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PartCategoryDto>.Failure(PartCategoryErrors.NameAlreadyExists);
        }

        var partCategory = new PartCategory
        {
            Name = normalizedName
        };

        await partCategoryRepository.AddAsync(partCategory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartCategoryDto>.Success(MapToDto(partCategory));
    }

    public async Task<Result<PartCategoryDto>> UpdateAsync(
        int id,
        UpdatePartCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var partCategory = await partCategoryRepository.GetByIdAsync(id, cancellationToken);

        if (partCategory is null)
        {
            return Result<PartCategoryDto>.Failure(PartCategoryErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PartCategoryDto>.Failure(validationError);
        }

        var nameAlreadyExists = await partCategoryRepository.ExistsAsync(
            x => x.Name == normalizedName && x.PartCategoryId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PartCategoryDto>.Failure(PartCategoryErrors.NameAlreadyExists);
        }

        partCategory.Name = normalizedName;

        partCategoryRepository.Update(partCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartCategoryDto>.Success(MapToDto(partCategory));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var partCategoryRepository = _unitOfWork.Repository<PartCategory>();
        var partCategory = await partCategoryRepository.GetByIdAsync(id, cancellationToken);

        if (partCategory is null)
        {
            return Result.Failure(PartCategoryErrors.NotFound);
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var inUse = await partRepository.ExistsAsync(
            x => x.PartCategoryId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(PartCategoryErrors.InUse);
        }

        partCategoryRepository.Remove(partCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PartCategoryDto MapToDto(PartCategory partCategory)
    {
        return new PartCategoryDto
        {
            PartCategoryId = partCategory.PartCategoryId,
            Name = partCategory.Name
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
            return PartCategoryErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return PartCategoryErrors.NameTooLong;
        }

        return null;
    }
}
