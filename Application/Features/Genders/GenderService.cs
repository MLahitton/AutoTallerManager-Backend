using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Genders.Dtos;
using Application.Features.Genders.Errors;
using Application.Features.Genders.Requests;
using Domain.Entities;

namespace Application.Features.Genders;

public class GenderService : IGenderService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public GenderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<GenderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genderRepository = _unitOfWork.Repository<Gender>();
        var genders = await genderRepository.GetAllAsync(cancellationToken);

        var genderDtos = genders
            .OrderBy(x => x.GenderId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<GenderDto>>.Success(genderDtos);
    }

    public async Task<Result<GenderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var genderRepository = _unitOfWork.Repository<Gender>();
        var gender = await genderRepository.GetByIdAsync(id, cancellationToken);

        if (gender is null)
        {
            return Result<GenderDto>.Failure(GenderErrors.NotFound);
        }

        return Result<GenderDto>.Success(MapToDto(gender));
    }

    public async Task<Result<GenderDto>> CreateAsync(CreateGenderRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<GenderDto>.Failure(validationError);
        }

        var genderRepository = _unitOfWork.Repository<Gender>();
        var nameAlreadyExists = await genderRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<GenderDto>.Failure(GenderErrors.NameAlreadyExists);
        }

        var gender = new Gender
        {
            Name = normalizedName
        };

        await genderRepository.AddAsync(gender, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<GenderDto>.Success(MapToDto(gender));
    }

    public async Task<Result<GenderDto>> UpdateAsync(
        int id,
        UpdateGenderRequest request,
        CancellationToken cancellationToken = default)
    {
        var genderRepository = _unitOfWork.Repository<Gender>();
        var gender = await genderRepository.GetByIdAsync(id, cancellationToken);

        if (gender is null)
        {
            return Result<GenderDto>.Failure(GenderErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<GenderDto>.Failure(validationError);
        }

        var nameAlreadyExists = await genderRepository.ExistsAsync(
            x => x.Name == normalizedName && x.GenderId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<GenderDto>.Failure(GenderErrors.NameAlreadyExists);
        }

        gender.Name = normalizedName;

        genderRepository.Update(gender);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<GenderDto>.Success(MapToDto(gender));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var genderRepository = _unitOfWork.Repository<Gender>();
        var gender = await genderRepository.GetByIdAsync(id, cancellationToken);

        if (gender is null)
        {
            return Result.Failure(GenderErrors.NotFound);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var inUse = await personRepository.ExistsAsync(
            x => x.GenderId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(GenderErrors.InUse);
        }

        genderRepository.Remove(gender);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static GenderDto MapToDto(Gender gender)
    {
        return new GenderDto
        {
            GenderId = gender.GenderId,
            Name = gender.Name
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
            return GenderErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return GenderErrors.NameTooLong;
        }

        return null;
    }
}
