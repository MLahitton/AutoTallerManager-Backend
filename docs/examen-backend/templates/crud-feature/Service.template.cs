// PLANTILLA DE ESTUDIO — NO COMPILAR
// Copiar a: Application/Features/NewEntities/NewEntityService.cs
//           Application/Features/NewEntities/INewEntityService.cs (interfaz con mismos métodos)
// Referencia: Application/Features/Genders/GenderService.cs (CRUD simple)
//             Application/Features/Vehicles/VehicleService.cs (validación + FKs)
// Registrar en: Application/DependencyInjection.cs → AddScoped<INewEntityService, NewEntityService>()

using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.NewEntities.Dtos;
using Application.Features.NewEntities.Errors;
using Application.Features.NewEntities.Requests;
using Domain.Entities;

namespace Application.Features.NewEntities;

public class NewEntityService : INewEntityService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public NewEntityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<NewEntityDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<NewEntity>();
        var items = await repository.GetAllAsync(cancellationToken);

        var dtos = items
            .OrderBy(x => x.NewEntityId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<NewEntityDto>>.Success(dtos);
    }

    public async Task<Result<NewEntityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<NewEntity>();
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return Result<NewEntityDto>.Failure(NewEntityErrors.NotFound);
        }

        return Result<NewEntityDto>.Success(MapToDto(entity));
    }

    public async Task<Result<NewEntityDto>> CreateAsync(
        CreateNewEntityRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(request?.Name);
        var validationError = ValidateName(name);
        if (validationError is not null)
        {
            return Result<NewEntityDto>.Failure(validationError);
        }

        var repository = _unitOfWork.Repository<NewEntity>();
        var nameExists = await repository.ExistsAsync(x => x.Name == name, cancellationToken);
        if (nameExists)
        {
            return Result<NewEntityDto>.Failure(NewEntityErrors.NameAlreadyExists);
        }

        var entity = new NewEntity
        {
            Name = name,
            IsActive = request?.IsActive ?? true
        };

        await repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NewEntityDto>.Success(MapToDto(entity));
    }

    public async Task<Result<NewEntityDto>> UpdateAsync(
        int id,
        UpdateNewEntityRequest request,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<NewEntity>();
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return Result<NewEntityDto>.Failure(NewEntityErrors.NotFound);
        }

        var name = NormalizeName(request?.Name);
        var validationError = ValidateName(name);
        if (validationError is not null)
        {
            return Result<NewEntityDto>.Failure(validationError);
        }

        var nameExists = await repository.ExistsAsync(
            x => x.Name == name && x.NewEntityId != id,
            cancellationToken);

        if (nameExists)
        {
            return Result<NewEntityDto>.Failure(NewEntityErrors.NameAlreadyExists);
        }

        entity.Name = name;
        entity.IsActive = request?.IsActive ?? entity.IsActive;

        repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NewEntityDto>.Success(MapToDto(entity));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<NewEntity>();
        var entity = await repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(NewEntityErrors.NotFound);
        }

        // Si otras tablas referencian esta entidad, verificar InUse:
        // var inUse = await _unitOfWork.Repository<Other>().ExistsAsync(x => x.NewEntityId == id, cancellationToken);
        // if (inUse) return Result.Failure(NewEntityErrors.InUse);

        repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static NewEntityDto MapToDto(NewEntity entity)
    {
        return new NewEntityDto
        {
            NewEntityId = entity.NewEntityId,
            Name = entity.Name,
            IsActive = entity.IsActive
        };
    }

    private static string NormalizeName(string? name) => (name ?? string.Empty).Trim();

    private static Error? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return NewEntityErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return NewEntityErrors.NameTooLong;
        }

        return null;
    }
}

// Interfaz (archivo separado INewEntityService.cs):
// public interface INewEntityService
// {
//     Task<Result<IReadOnlyList<NewEntityDto>>> GetAllAsync(CancellationToken cancellationToken = default);
//     Task<Result<NewEntityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
//     Task<Result<NewEntityDto>> CreateAsync(CreateNewEntityRequest request, CancellationToken cancellationToken = default);
//     Task<Result<NewEntityDto>> UpdateAsync(int id, UpdateNewEntityRequest request, CancellationToken cancellationToken = default);
//     Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
// }
