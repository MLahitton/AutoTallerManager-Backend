using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.WorkshopIntake.Dtos;
using Application.Features.WorkshopIntake.Errors;
using Application.Features.WorkshopIntake.Requests;
using Domain.Entities;

namespace Application.Features.WorkshopIntake;

public class WorkshopIntakeService : IWorkshopIntakeService
{
    private const string PendingStatusName = "Pending";
    private const string InProgressStatusName = "InProgress";
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";
    private const string InitialIntakeObservation = "Initial service order intake.";

    private readonly IUnitOfWork _unitOfWork;

    public WorkshopIntakeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkshopIntakeDto>> CreateServiceOrderAsync(
        int changedByUserId,
        CreateWorkshopIntakeRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleId = request?.VehicleId ?? 0;
        var initialOrderStatusId = request?.InitialOrderStatusId;
        var entryDate = request?.EntryDate ?? DateTime.UtcNow;
        var estimatedDeliveryDate = request?.EstimatedDeliveryDate;
        var generalDescription = NormalizeOptionalText(request?.GeneralDescription);

        var hasScratches = request?.HasScratches ?? false;
        var scratchesDescription = NormalizeOptionalText(request?.ScratchesDescription);
        var hasToolbox = request?.HasToolbox ?? false;
        var toolboxDescription = NormalizeOptionalText(request?.ToolboxDescription);
        var ownershipCardDelivered = request?.OwnershipCardDelivered ?? false;
        var inventoryObservations = NormalizeOptionalText(request?.InventoryObservations);
        var requestedServices = request?.Services?.ToList() ?? new List<CreateWorkshopIntakeOrderServiceRequest>();

        var validationError = Validate(
            changedByUserId,
            vehicleId,
            initialOrderStatusId,
            entryDate,
            estimatedDeliveryDate,
            hasScratches,
            scratchesDescription,
            hasToolbox,
            toolboxDescription,
            requestedServices);

        if (validationError is not null)
        {
            return Result<WorkshopIntakeDto>.Failure(validationError);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var changedByUserExists = await userRepository.ExistsAsync(
            x => x.UserId == changedByUserId,
            cancellationToken);

        if (!changedByUserExists)
        {
            return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.ChangedByUserNotFound);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.VehicleNotFound);
        }

        if (!vehicle.IsActive)
        {
            return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.VehicleInactive);
        }

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatuses = await orderStatusRepository.GetAllAsync(cancellationToken);

        var pendingStatus = orderStatuses.FirstOrDefault(x =>
            x.Name.Equals(PendingStatusName, StringComparison.OrdinalIgnoreCase));
        if (pendingStatus is null)
        {
            return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.PendingStatusNotFound);
        }

        var initialStatus = initialOrderStatusId.HasValue
            ? orderStatuses.FirstOrDefault(x => x.OrderStatusId == initialOrderStatusId.Value)
            : pendingStatus;

        if (initialStatus is null)
        {
            return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.InitialOrderStatusNotFound);
        }

        if (initialStatus.Name.Equals(CancelledStatusName, StringComparison.OrdinalIgnoreCase) ||
            initialStatus.Name.Equals(VoidedStatusName, StringComparison.OrdinalIgnoreCase))
        {
            return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.InitialOrderStatusInvalidConflict);
        }

        var activeStatusIds = orderStatuses
            .Where(x => x.Name.Equals(PendingStatusName, StringComparison.OrdinalIgnoreCase) ||
                        x.Name.Equals(InProgressStatusName, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.OrderStatusId)
            .Distinct()
            .ToArray();

        if (activeStatusIds.Contains(initialStatus.OrderStatusId))
        {
            var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
            var activeOrderAlreadyExists = await serviceOrderRepository.ExistsAsync(
                x => x.VehicleId == vehicleId && activeStatusIds.Contains(x.OrderStatusId),
                cancellationToken);

            if (activeOrderAlreadyExists)
            {
                return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.ActiveOrderAlreadyExistsConflict);
            }
        }

        var requestedServiceTypeIds = requestedServices
            .Select(x => x.ServiceTypeId)
            .Distinct()
            .ToList();

        if (requestedServiceTypeIds.Count > 0)
        {
            var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
            var serviceTypes = await serviceTypeRepository.GetAllAsync(cancellationToken);
            var serviceTypeIdSet = serviceTypes.Select(x => x.ServiceTypeId).ToHashSet();

            if (requestedServiceTypeIds.Any(x => !serviceTypeIdSet.Contains(x)))
            {
                return Result<WorkshopIntakeDto>.Failure(WorkshopIntakeErrors.ServiceTypeNotFound);
            }
        }

        if (!hasScratches)
        {
            scratchesDescription = null;
        }

        if (!hasToolbox)
        {
            toolboxDescription = null;
        }

        var serviceOrder = new ServiceOrder
        {
            VehicleId = vehicleId,
            OrderStatusId = initialStatus.OrderStatusId,
            EntryDate = entryDate,
            EstimatedDeliveryDate = estimatedDeliveryDate,
            GeneralDescription = generalDescription,
            CancellationReason = null,
            CancellationDate = null
        };

        var serviceOrderRepositoryForCreate = _unitOfWork.Repository<ServiceOrder>();
        await serviceOrderRepositoryForCreate.AddAsync(serviceOrder, cancellationToken);

        var inventory = new VehicleEntryInventory
        {
            ServiceOrder = serviceOrder,
            HasScratches = hasScratches,
            ScratchesDescription = scratchesDescription,
            HasToolbox = hasToolbox,
            ToolboxDescription = toolboxDescription,
            OwnershipCardDelivered = ownershipCardDelivered,
            Observations = inventoryObservations,
            RegisteredAt = DateTime.UtcNow
        };

        var inventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        await inventoryRepository.AddAsync(inventory, cancellationToken);

        var statusHistory = new OrderStatusHistory
        {
            ServiceOrder = serviceOrder,
            PreviousOrderStatusId = null,
            NewOrderStatusId = initialStatus.OrderStatusId,
            ChangedByUserId = changedByUserId,
            Observation = InitialIntakeObservation,
            ChangedAt = DateTime.UtcNow
        };

        var statusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        await statusHistoryRepository.AddAsync(statusHistory, cancellationToken);

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var createdOrderServices = new List<OrderService>();

        foreach (var requestedService in requestedServices)
        {
            var orderService = new OrderService
            {
                ServiceOrder = serviceOrder,
                ServiceTypeId = requestedService.ServiceTypeId,
                Description = NormalizeOptionalText(requestedService.Description),
                WorkPerformed = null,
                LaborCost = requestedService.LaborCost,
                CustomerApproved = null,
                ApprovalDate = null
            };

            await orderServiceRepository.AddAsync(orderService, cancellationToken);
            createdOrderServices.Add(orderService);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WorkshopIntakeDto>.Success(new WorkshopIntakeDto
        {
            ServiceOrderId = serviceOrder.ServiceOrderId,
            VehicleId = serviceOrder.VehicleId,
            OrderStatusId = serviceOrder.OrderStatusId,
            EntryInventoryId = inventory.EntryInventoryId,
            OrderStatusHistoryId = statusHistory.OrderStatusHistoryId,
            EntryDate = serviceOrder.EntryDate,
            EstimatedDeliveryDate = serviceOrder.EstimatedDeliveryDate,
            GeneralDescription = serviceOrder.GeneralDescription,
            Services = createdOrderServices
                .OrderBy(x => x.OrderServiceId)
                .Select(x => new WorkshopIntakeOrderServiceDto
                {
                    OrderServiceId = x.OrderServiceId,
                    ServiceTypeId = x.ServiceTypeId,
                    Description = x.Description,
                    LaborCost = x.LaborCost
                })
                .ToList()
        });
    }

    private static Error? Validate(
        int changedByUserId,
        int vehicleId,
        int? initialOrderStatusId,
        DateTime entryDate,
        DateTime? estimatedDeliveryDate,
        bool hasScratches,
        string? scratchesDescription,
        bool hasToolbox,
        string? toolboxDescription,
        IReadOnlyList<CreateWorkshopIntakeOrderServiceRequest> services)
    {
        if (changedByUserId <= 0)
        {
            return WorkshopIntakeErrors.ChangedByUserIdInvalid;
        }

        if (vehicleId <= 0)
        {
            return WorkshopIntakeErrors.VehicleIdInvalid;
        }

        if (initialOrderStatusId.HasValue && initialOrderStatusId.Value <= 0)
        {
            return WorkshopIntakeErrors.InitialOrderStatusIdInvalid;
        }

        if (entryDate == default)
        {
            return WorkshopIntakeErrors.EntryDateInvalid;
        }

        if (estimatedDeliveryDate.HasValue && estimatedDeliveryDate.Value < entryDate)
        {
            return WorkshopIntakeErrors.EstimatedDeliveryDateInvalid;
        }

        if (hasScratches && string.IsNullOrWhiteSpace(scratchesDescription))
        {
            return WorkshopIntakeErrors.ScratchesDescriptionRequired;
        }

        if (hasToolbox && string.IsNullOrWhiteSpace(toolboxDescription))
        {
            return WorkshopIntakeErrors.ToolboxDescriptionRequired;
        }

        foreach (var service in services)
        {
            if (service.ServiceTypeId <= 0)
            {
                return WorkshopIntakeErrors.ServiceTypeIdInvalid;
            }

            if (service.LaborCost < 0m)
            {
                return WorkshopIntakeErrors.LaborCostInvalid;
            }
        }

        return null;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
