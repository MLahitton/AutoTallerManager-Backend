using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Departments.Dtos;
using Application.Features.Departments.Errors;
using Application.Features.Departments.Requests;
using Domain.Entities;

namespace Application.Features.Departments;

public class DepartmentService : IDepartmentService
{
    private const int NameMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departmentRepository = _unitOfWork.Repository<Department>();
        var departments = await departmentRepository.GetAllAsync(cancellationToken);

        var departmentDtos = departments
            .OrderBy(x => x.DepartmentId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<DepartmentDto>>.Success(departmentDtos);
    }

    public async Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var departmentRepository = _unitOfWork.Repository<Department>();
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
        {
            return Result<DepartmentDto>.Failure(DepartmentErrors.NotFound);
        }

        return Result<DepartmentDto>.Success(MapToDto(department));
    }

    public async Task<Result<DepartmentDto>> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var countryId = request?.CountryId ?? 0;
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(countryId, normalizedName);
        if (validationError is not null)
        {
            return Result<DepartmentDto>.Failure(validationError);
        }

        var countryRepository = _unitOfWork.Repository<Country>();
        var countryExists = await countryRepository.ExistsAsync(
            x => x.CountryId == countryId,
            cancellationToken);

        if (!countryExists)
        {
            return Result<DepartmentDto>.Failure(DepartmentErrors.CountryNotFound);
        }

        var departmentRepository = _unitOfWork.Repository<Department>();
        var nameAlreadyExists = await departmentRepository.ExistsAsync(
            x => x.CountryId == countryId && x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<DepartmentDto>.Failure(DepartmentErrors.NameAlreadyExists);
        }

        var department = new Department
        {
            CountryId = countryId,
            Name = normalizedName
        };

        await departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DepartmentDto>.Success(MapToDto(department));
    }

    public async Task<Result<DepartmentDto>> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var departmentRepository = _unitOfWork.Repository<Department>();
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
        {
            return Result<DepartmentDto>.Failure(DepartmentErrors.NotFound);
        }

        var countryId = request?.CountryId ?? 0;
        var normalizedName = NormalizeName(request?.Name);

        var validationError = Validate(countryId, normalizedName);
        if (validationError is not null)
        {
            return Result<DepartmentDto>.Failure(validationError);
        }

        var countryRepository = _unitOfWork.Repository<Country>();
        var countryExists = await countryRepository.ExistsAsync(
            x => x.CountryId == countryId,
            cancellationToken);

        if (!countryExists)
        {
            return Result<DepartmentDto>.Failure(DepartmentErrors.CountryNotFound);
        }

        var nameAlreadyExists = await departmentRepository.ExistsAsync(
            x => x.CountryId == countryId && x.Name == normalizedName && x.DepartmentId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<DepartmentDto>.Failure(DepartmentErrors.NameAlreadyExists);
        }

        department.CountryId = countryId;
        department.Name = normalizedName;

        departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DepartmentDto>.Success(MapToDto(department));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var departmentRepository = _unitOfWork.Repository<Department>();
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
        {
            return Result.Failure(DepartmentErrors.NotFound);
        }

        var cityRepository = _unitOfWork.Repository<City>();
        var inUse = await cityRepository.ExistsAsync(
            x => x.DepartmentId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(DepartmentErrors.InUse);
        }

        departmentRepository.Remove(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            CountryId = department.CountryId,
            Name = department.Name
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? Validate(int countryId, string name)
    {
        if (countryId <= 0)
        {
            return DepartmentErrors.CountryIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return DepartmentErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return DepartmentErrors.NameTooLong;
        }

        return null;
    }
}
