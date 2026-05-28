using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PartBrands.Dtos;
using Application.Features.PartBrands.Errors;
using Application.Features.PartBrands.Requests;
using Domain.Entities;

namespace Application.Features.PartBrands;

public class PartBrandService : IPartBrandService
{
    private const int NameMaxLength = 80;
    private readonly IUnitOfWork _unitOfWork;

    public PartBrandService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PartBrandDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partBrandRepository = _unitOfWork.Repository<PartBrand>();
        var partBrands = await partBrandRepository.GetAllAsync(cancellationToken);

        var partBrandDtos = partBrands
            .OrderBy(x => x.PartBrandId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PartBrandDto>>.Success(partBrandDtos);
    }

    public async Task<Result<PartBrandDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partBrandRepository = _unitOfWork.Repository<PartBrand>();
        var partBrand = await partBrandRepository.GetByIdAsync(id, cancellationToken);

        if (partBrand is null)
        {
            return Result<PartBrandDto>.Failure(PartBrandErrors.NotFound);
        }

        return Result<PartBrandDto>.Success(MapToDto(partBrand));
    }

    public async Task<Result<PartBrandDto>> CreateAsync(
        CreatePartBrandRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PartBrandDto>.Failure(validationError);
        }

        var partBrandRepository = _unitOfWork.Repository<PartBrand>();
        var nameAlreadyExists = await partBrandRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PartBrandDto>.Failure(PartBrandErrors.NameAlreadyExists);
        }

        var partBrand = new PartBrand
        {
            Name = normalizedName
        };

        await partBrandRepository.AddAsync(partBrand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartBrandDto>.Success(MapToDto(partBrand));
    }

    public async Task<Result<PartBrandDto>> UpdateAsync(
        int id,
        UpdatePartBrandRequest request,
        CancellationToken cancellationToken = default)
    {
        var partBrandRepository = _unitOfWork.Repository<PartBrand>();
        var partBrand = await partBrandRepository.GetByIdAsync(id, cancellationToken);

        if (partBrand is null)
        {
            return Result<PartBrandDto>.Failure(PartBrandErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PartBrandDto>.Failure(validationError);
        }

        var nameAlreadyExists = await partBrandRepository.ExistsAsync(
            x => x.Name == normalizedName && x.PartBrandId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PartBrandDto>.Failure(PartBrandErrors.NameAlreadyExists);
        }

        partBrand.Name = normalizedName;

        partBrandRepository.Update(partBrand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PartBrandDto>.Success(MapToDto(partBrand));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var partBrandRepository = _unitOfWork.Repository<PartBrand>();
        var partBrand = await partBrandRepository.GetByIdAsync(id, cancellationToken);

        if (partBrand is null)
        {
            return Result.Failure(PartBrandErrors.NotFound);
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var inUse = await partRepository.ExistsAsync(
            x => x.PartBrandId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(PartBrandErrors.InUse);
        }

        partBrandRepository.Remove(partBrand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PartBrandDto MapToDto(PartBrand partBrand)
    {
        return new PartBrandDto
        {
            PartBrandId = partBrand.PartBrandId,
            Name = partBrand.Name
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
            return PartBrandErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return PartBrandErrors.NameTooLong;
        }

        return null;
    }
}
