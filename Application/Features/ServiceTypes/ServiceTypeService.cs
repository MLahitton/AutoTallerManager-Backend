using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.ServiceTypes.Dtos;
using Application.Features.ServiceTypes.Errors;
using Application.Features.ServiceTypes.Requests;
using Domain.Entities;

namespace Application.Features.ServiceTypes;

public class ServiceTypeService : IServiceTypeService
{
    private const int NameMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ServiceTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceTypes = await serviceTypeRepository.GetAllAsync(cancellationToken);

        var serviceTypeDtos = serviceTypes
            .OrderBy(x => x.ServiceTypeId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<ServiceTypeDto>>.Success(serviceTypeDtos);
    }

    public async Task<Result<ServiceTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceType = await serviceTypeRepository.GetByIdAsync(id, cancellationToken);

        if (serviceType is null)
        {
            return Result<ServiceTypeDto>.Failure(ServiceTypeErrors.NotFound);
        }

        return Result<ServiceTypeDto>.Success(MapToDto(serviceType));
    }

    public async Task<Result<ServiceTypeDto>> CreateAsync(
        CreateServiceTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var estimatedDays = request?.EstimatedDays ?? 0;

        var validationError = Validate(normalizedName, estimatedDays);
        if (validationError is not null)
        {
            return Result<ServiceTypeDto>.Failure(validationError);
        }

        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var nameAlreadyExists = await serviceTypeRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<ServiceTypeDto>.Failure(ServiceTypeErrors.NameAlreadyExists);
        }

        var serviceType = new ServiceType
        {
            Name = normalizedName,
            EstimatedDays = estimatedDays
        };

        await serviceTypeRepository.AddAsync(serviceType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceTypeDto>.Success(MapToDto(serviceType));
    }

    public async Task<Result<ServiceTypeDto>> UpdateAsync(
        int id,
        UpdateServiceTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceType = await serviceTypeRepository.GetByIdAsync(id, cancellationToken);

        if (serviceType is null)
        {
            return Result<ServiceTypeDto>.Failure(ServiceTypeErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var estimatedDays = request?.EstimatedDays ?? 0;

        var validationError = Validate(normalizedName, estimatedDays);
        if (validationError is not null)
        {
            return Result<ServiceTypeDto>.Failure(validationError);
        }

        var nameAlreadyExists = await serviceTypeRepository.ExistsAsync(
            x => x.Name == normalizedName && x.ServiceTypeId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<ServiceTypeDto>.Failure(ServiceTypeErrors.NameAlreadyExists);
        }

        serviceType.Name = normalizedName;
        serviceType.EstimatedDays = estimatedDays;

        serviceTypeRepository.Update(serviceType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceTypeDto>.Success(MapToDto(serviceType));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceType = await serviceTypeRepository.GetByIdAsync(id, cancellationToken);

        if (serviceType is null)
        {
            return Result.Failure(ServiceTypeErrors.NotFound);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var inUse = await orderServiceRepository.ExistsAsync(
            x => x.ServiceTypeId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(ServiceTypeErrors.InUse);
        }

        serviceTypeRepository.Remove(serviceType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ServiceTypeDto MapToDto(ServiceType serviceType)
    {
        return new ServiceTypeDto
        {
            ServiceTypeId = serviceType.ServiceTypeId,
            Name = serviceType.Name,
            EstimatedDays = serviceType.EstimatedDays
        };
    }

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    private static Error? Validate(string name, int estimatedDays)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceTypeErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return ServiceTypeErrors.NameTooLong;
        }

        if (estimatedDays < 1)
        {
            return ServiceTypeErrors.EstimatedDaysInvalid;
        }

        return null;
    }
}
