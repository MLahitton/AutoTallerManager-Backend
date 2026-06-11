using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.parte_nueva.Dtos;
using Application.Features.parte_nueva.Errors;
using Application.Features.parte_nueva.Requests;
using Domain.Entities;

namespace Application.Features.parte_nueva;

public class parte_nuevaService : Iparte_nuevaService
{
    private const int NameMaxLength = 100;
    private const int DescriptionMaxLength = 500;

    private readonly IUnitOfWork _unitOfWork;

    public parte_nuevaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<parte_nuevaDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Domain.Entities.parte_nueva>();
        var items = await repository.GetAllAsync(cancellationToken);

        var dtos = items
            .OrderBy(x => x.parte_nuevaId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<parte_nuevaDto>>.Success(dtos);
    }

    public async Task<Result<parte_nuevaDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Result<parte_nuevaDto>.Failure(parte_nuevaErrors.IdInvalid);
        }

        var repository = _unitOfWork.Repository<Domain.Entities.parte_nueva>();
        var item = await repository.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return Result<parte_nuevaDto>.Failure(parte_nuevaErrors.NotFound);
        }

        return Result<parte_nuevaDto>.Success(MapToDto(item));
    }

    public async Task<Result<parte_nuevaDto>> CreateAsync(
        Createparte_nuevaRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeRequiredText(request?.Name);
        var normalizedDescription = NormalizeOptionalText(request?.Description);

        var validationError = Validate(normalizedName, normalizedDescription);
        if (validationError is not null)
        {
            return Result<parte_nuevaDto>.Failure(validationError);
        }

        var repository = _unitOfWork.Repository<Domain.Entities.parte_nueva>();
        var nameAlreadyExists = await repository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<parte_nuevaDto>.Failure(parte_nuevaErrors.NameAlreadyExists);
        }

        var item = new Domain.Entities.parte_nueva
        {
            Name = normalizedName,
            Description = normalizedDescription,
            IsActive = request?.IsActive ?? true
        };

        await repository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<parte_nuevaDto>.Success(MapToDto(item));
    }

    public async Task<Result<parte_nuevaDto>> UpdateAsync(
        int id,
        Updateparte_nuevaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Result<parte_nuevaDto>.Failure(parte_nuevaErrors.IdInvalid);
        }

        var repository = _unitOfWork.Repository<Domain.Entities.parte_nueva>();
        var item = await repository.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return Result<parte_nuevaDto>.Failure(parte_nuevaErrors.NotFound);
        }

        var normalizedName = NormalizeRequiredText(request?.Name);
        var normalizedDescription = NormalizeOptionalText(request?.Description);

        var validationError = Validate(normalizedName, normalizedDescription);
        if (validationError is not null)
        {
            return Result<parte_nuevaDto>.Failure(validationError);
        }

        var nameAlreadyExists = await repository.ExistsAsync(
            x => x.Name == normalizedName && x.parte_nuevaId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<parte_nuevaDto>.Failure(parte_nuevaErrors.NameAlreadyExists);
        }

        item.Name = normalizedName;
        item.Description = normalizedDescription;
        item.IsActive = request?.IsActive ?? item.IsActive;

        repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<parte_nuevaDto>.Success(MapToDto(item));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Result.Failure(parte_nuevaErrors.IdInvalid);
        }

        var repository = _unitOfWork.Repository<Domain.Entities.parte_nueva>();
        var item = await repository.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(parte_nuevaErrors.NotFound);
        }

        repository.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static parte_nuevaDto MapToDto(Domain.Entities.parte_nueva item)
    {
        return new parte_nuevaDto
        {
            parte_nuevaId = item.parte_nuevaId,
            Name = item.Name,
            Description = item.Description,
            IsActive = item.IsActive
        };
    }

    private static Error? Validate(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return parte_nuevaErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return parte_nuevaErrors.NameTooLong;
        }

        if (description is not null && description.Length > DescriptionMaxLength)
        {
            return parte_nuevaErrors.DescriptionTooLong;
        }

        return null;
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
