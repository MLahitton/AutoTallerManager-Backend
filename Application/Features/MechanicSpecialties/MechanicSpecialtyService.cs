using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.MechanicSpecialties.Dtos;
using Application.Features.MechanicSpecialties.Errors;
using Application.Features.MechanicSpecialties.Requests;
using Domain.Entities;

namespace Application.Features.MechanicSpecialties;

public class MechanicSpecialtyService : IMechanicSpecialtyService
{
    private const int NameMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public MechanicSpecialtyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<MechanicSpecialtyDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var mechanicSpecialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var mechanicSpecialties = await mechanicSpecialtyRepository.GetAllAsync(cancellationToken);

        var mechanicSpecialtyDtos = mechanicSpecialties
            .OrderBy(x => x.SpecialtyId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<MechanicSpecialtyDto>>.Success(mechanicSpecialtyDtos);
    }

    public async Task<Result<MechanicSpecialtyDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var mechanicSpecialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var mechanicSpecialty = await mechanicSpecialtyRepository.GetByIdAsync(id, cancellationToken);

        if (mechanicSpecialty is null)
        {
            return Result<MechanicSpecialtyDto>.Failure(MechanicSpecialtyErrors.NotFound);
        }

        return Result<MechanicSpecialtyDto>.Success(MapToDto(mechanicSpecialty));
    }

    public async Task<Result<MechanicSpecialtyDto>> CreateAsync(
        CreateMechanicSpecialtyRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<MechanicSpecialtyDto>.Failure(validationError);
        }

        var mechanicSpecialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var nameAlreadyExists = await mechanicSpecialtyRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<MechanicSpecialtyDto>.Failure(MechanicSpecialtyErrors.NameAlreadyExists);
        }

        var mechanicSpecialty = new MechanicSpecialty
        {
            Name = normalizedName
        };

        await mechanicSpecialtyRepository.AddAsync(mechanicSpecialty, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MechanicSpecialtyDto>.Success(MapToDto(mechanicSpecialty));
    }

    public async Task<Result<MechanicSpecialtyDto>> UpdateAsync(
        int id,
        UpdateMechanicSpecialtyRequest request,
        CancellationToken cancellationToken = default)
    {
        var mechanicSpecialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var mechanicSpecialty = await mechanicSpecialtyRepository.GetByIdAsync(id, cancellationToken);

        if (mechanicSpecialty is null)
        {
            return Result<MechanicSpecialtyDto>.Failure(MechanicSpecialtyErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<MechanicSpecialtyDto>.Failure(validationError);
        }

        var nameAlreadyExists = await mechanicSpecialtyRepository.ExistsAsync(
            x => x.Name == normalizedName && x.SpecialtyId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<MechanicSpecialtyDto>.Failure(MechanicSpecialtyErrors.NameAlreadyExists);
        }

        mechanicSpecialty.Name = normalizedName;

        mechanicSpecialtyRepository.Update(mechanicSpecialty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MechanicSpecialtyDto>.Success(MapToDto(mechanicSpecialty));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var mechanicSpecialtyRepository = _unitOfWork.Repository<MechanicSpecialty>();
        var mechanicSpecialty = await mechanicSpecialtyRepository.GetByIdAsync(id, cancellationToken);

        if (mechanicSpecialty is null)
        {
            return Result.Failure(MechanicSpecialtyErrors.NotFound);
        }

        var mechanicSpecialtyAssignmentRepository = _unitOfWork.Repository<MechanicSpecialtyAssignment>();
        var inUseByAssignment = await mechanicSpecialtyAssignmentRepository.ExistsAsync(
            x => x.SpecialtyId == id,
            cancellationToken);

        if (inUseByAssignment)
        {
            return Result.Failure(MechanicSpecialtyErrors.InUse);
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var inUseByMechanicAssignment = await mechanicAssignmentRepository.ExistsAsync(
            x => x.SpecialtyId == id,
            cancellationToken);

        if (inUseByMechanicAssignment)
        {
            return Result.Failure(MechanicSpecialtyErrors.InUse);
        }

        mechanicSpecialtyRepository.Remove(mechanicSpecialty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static MechanicSpecialtyDto MapToDto(MechanicSpecialty mechanicSpecialty)
    {
        return new MechanicSpecialtyDto
        {
            SpecialtyId = mechanicSpecialty.SpecialtyId,
            Name = mechanicSpecialty.Name
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
            return MechanicSpecialtyErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return MechanicSpecialtyErrors.NameTooLong;
        }

        return null;
    }
}
