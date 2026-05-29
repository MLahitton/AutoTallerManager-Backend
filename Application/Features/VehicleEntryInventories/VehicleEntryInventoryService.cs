using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.VehicleEntryInventories.Dtos;
using Application.Features.VehicleEntryInventories.Errors;
using Application.Features.VehicleEntryInventories.Requests;
using Domain.Entities;

namespace Application.Features.VehicleEntryInventories;

public class VehicleEntryInventoryService : IVehicleEntryInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public VehicleEntryInventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VehicleEntryInventoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleEntryInventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var vehicleEntryInventories = await vehicleEntryInventoryRepository.GetAllAsync(cancellationToken);

        var vehicleEntryInventoryDtos = vehicleEntryInventories
            .OrderBy(x => x.EntryInventoryId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<VehicleEntryInventoryDto>>.Success(vehicleEntryInventoryDtos);
    }

    public async Task<Result<VehicleEntryInventoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleEntryInventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var vehicleEntryInventory = await vehicleEntryInventoryRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleEntryInventory is null)
        {
            return Result<VehicleEntryInventoryDto>.Failure(VehicleEntryInventoryErrors.NotFound);
        }

        return Result<VehicleEntryInventoryDto>.Success(MapToDto(vehicleEntryInventory));
    }

    public async Task<Result<VehicleEntryInventoryDto>> CreateAsync(
        CreateVehicleEntryInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var hasScratches = request?.HasScratches ?? false;
        var scratchesDescription = NormalizeOptionalText(request?.ScratchesDescription);
        var hasToolbox = request?.HasToolbox ?? false;
        var toolboxDescription = NormalizeOptionalText(request?.ToolboxDescription);
        var ownershipCardDelivered = request?.OwnershipCardDelivered ?? false;
        var observations = NormalizeOptionalText(request?.Observations);

        var validationError = Validate(serviceOrderId, hasScratches, scratchesDescription, hasToolbox, toolboxDescription);
        if (validationError is not null)
        {
            return Result<VehicleEntryInventoryDto>.Failure(validationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrderExists = await serviceOrderRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (!serviceOrderExists)
        {
            return Result<VehicleEntryInventoryDto>.Failure(VehicleEntryInventoryErrors.ServiceOrderNotFound);
        }

        var vehicleEntryInventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var alreadyExistsForServiceOrder = await vehicleEntryInventoryRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (alreadyExistsForServiceOrder)
        {
            return Result<VehicleEntryInventoryDto>.Failure(VehicleEntryInventoryErrors.ServiceOrderAlreadyExists);
        }

        if (!hasScratches)
        {
            scratchesDescription = null;
        }

        if (!hasToolbox)
        {
            toolboxDescription = null;
        }

        var vehicleEntryInventory = new VehicleEntryInventory
        {
            ServiceOrderId = serviceOrderId,
            HasScratches = hasScratches,
            ScratchesDescription = scratchesDescription,
            HasToolbox = hasToolbox,
            ToolboxDescription = toolboxDescription,
            OwnershipCardDelivered = ownershipCardDelivered,
            Observations = observations,
            RegisteredAt = DateTime.UtcNow
        };

        await vehicleEntryInventoryRepository.AddAsync(vehicleEntryInventory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleEntryInventoryDto>.Success(MapToDto(vehicleEntryInventory));
    }

    public async Task<Result<VehicleEntryInventoryDto>> UpdateAsync(
        int id,
        UpdateVehicleEntryInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleEntryInventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var vehicleEntryInventory = await vehicleEntryInventoryRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleEntryInventory is null)
        {
            return Result<VehicleEntryInventoryDto>.Failure(VehicleEntryInventoryErrors.NotFound);
        }

        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var hasScratches = request?.HasScratches ?? false;
        var scratchesDescription = NormalizeOptionalText(request?.ScratchesDescription);
        var hasToolbox = request?.HasToolbox ?? false;
        var toolboxDescription = NormalizeOptionalText(request?.ToolboxDescription);
        var ownershipCardDelivered = request?.OwnershipCardDelivered ?? false;
        var observations = NormalizeOptionalText(request?.Observations);

        var validationError = Validate(serviceOrderId, hasScratches, scratchesDescription, hasToolbox, toolboxDescription);
        if (validationError is not null)
        {
            return Result<VehicleEntryInventoryDto>.Failure(validationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrderExists = await serviceOrderRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (!serviceOrderExists)
        {
            return Result<VehicleEntryInventoryDto>.Failure(VehicleEntryInventoryErrors.ServiceOrderNotFound);
        }

        var alreadyExistsForServiceOrder = await vehicleEntryInventoryRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId && x.EntryInventoryId != id,
            cancellationToken);

        if (alreadyExistsForServiceOrder)
        {
            return Result<VehicleEntryInventoryDto>.Failure(VehicleEntryInventoryErrors.ServiceOrderAlreadyExists);
        }

        if (!hasScratches)
        {
            scratchesDescription = null;
        }

        if (!hasToolbox)
        {
            toolboxDescription = null;
        }

        vehicleEntryInventory.ServiceOrderId = serviceOrderId;
        vehicleEntryInventory.HasScratches = hasScratches;
        vehicleEntryInventory.ScratchesDescription = scratchesDescription;
        vehicleEntryInventory.HasToolbox = hasToolbox;
        vehicleEntryInventory.ToolboxDescription = toolboxDescription;
        vehicleEntryInventory.OwnershipCardDelivered = ownershipCardDelivered;
        vehicleEntryInventory.Observations = observations;

        vehicleEntryInventoryRepository.Update(vehicleEntryInventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleEntryInventoryDto>.Success(MapToDto(vehicleEntryInventory));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicleEntryInventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var vehicleEntryInventory = await vehicleEntryInventoryRepository.GetByIdAsync(id, cancellationToken);

        if (vehicleEntryInventory is null)
        {
            return Result.Failure(VehicleEntryInventoryErrors.NotFound);
        }

        vehicleEntryInventoryRepository.Remove(vehicleEntryInventory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleEntryInventoryDto MapToDto(VehicleEntryInventory vehicleEntryInventory)
    {
        return new VehicleEntryInventoryDto
        {
            EntryInventoryId = vehicleEntryInventory.EntryInventoryId,
            ServiceOrderId = vehicleEntryInventory.ServiceOrderId,
            HasScratches = vehicleEntryInventory.HasScratches,
            ScratchesDescription = vehicleEntryInventory.ScratchesDescription,
            HasToolbox = vehicleEntryInventory.HasToolbox,
            ToolboxDescription = vehicleEntryInventory.ToolboxDescription,
            OwnershipCardDelivered = vehicleEntryInventory.OwnershipCardDelivered,
            Observations = vehicleEntryInventory.Observations,
            RegisteredAt = vehicleEntryInventory.RegisteredAt
        };
    }

    private static Error? Validate(
        int serviceOrderId,
        bool hasScratches,
        string? scratchesDescription,
        bool hasToolbox,
        string? toolboxDescription)
    {
        if (serviceOrderId <= 0)
        {
            return VehicleEntryInventoryErrors.ServiceOrderIdInvalid;
        }

        if (hasScratches && string.IsNullOrWhiteSpace(scratchesDescription))
        {
            return VehicleEntryInventoryErrors.ScratchesDescriptionRequired;
        }

        if (hasToolbox && string.IsNullOrWhiteSpace(toolboxDescription))
        {
            return VehicleEntryInventoryErrors.ToolboxDescriptionRequired;
        }

        return null;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
