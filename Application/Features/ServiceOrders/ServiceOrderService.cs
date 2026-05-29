using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.ServiceOrders.Dtos;
using Application.Features.ServiceOrders.Errors;
using Application.Features.ServiceOrders.Requests;
using Domain.Entities;

namespace Application.Features.ServiceOrders;

public class ServiceOrderService : IServiceOrderService
{
    private static readonly DateTime MinimumEntryDate = new(1900, 1, 1);

    private const string PendingStatusName = "Pending";
    private const string InProgressStatusName = "InProgress";
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";

    private readonly IUnitOfWork _unitOfWork;

    public ServiceOrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ServiceOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrders = await serviceOrderRepository.GetAllAsync(cancellationToken);

        var serviceOrderDtos = serviceOrders
            .OrderBy(x => x.ServiceOrderId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<ServiceOrderDto>>.Success(serviceOrderDtos);
    }

    public async Task<Result<ServiceOrderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(id, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.NotFound);
        }

        return Result<ServiceOrderDto>.Success(MapToDto(serviceOrder));
    }

    public async Task<Result<ServiceOrderDto>> CreateAsync(
        CreateServiceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleId = request?.VehicleId ?? 0;
        var orderStatusId = request?.OrderStatusId ?? 0;
        var entryDate = NormalizeEntryDate(request?.EntryDate);
        var estimatedDeliveryDate = request?.EstimatedDeliveryDate;
        var generalDescription = NormalizeOptionalText(request?.GeneralDescription);

        var validationError = Validate(vehicleId, orderStatusId, entryDate, estimatedDeliveryDate);
        if (validationError is not null)
        {
            return Result<ServiceOrderDto>.Failure(validationError);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.VehicleNotFound);
        }

        if (!vehicle.IsActive)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.VehicleInactive);
        }

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatus = await orderStatusRepository.GetByIdAsync(orderStatusId, cancellationToken);

        if (orderStatus is null)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.OrderStatusNotFound);
        }

        var activeOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { PendingStatusName, InProgressStatusName },
            cancellationToken);

        if (activeOrderStatusIds.Contains(orderStatusId))
        {
            var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
            var activeOrderAlreadyExists = await serviceOrderRepository.ExistsAsync(
                x => x.VehicleId == vehicleId && activeOrderStatusIds.Contains(x.OrderStatusId),
                cancellationToken);

            if (activeOrderAlreadyExists)
            {
                return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.ActiveOrderAlreadyExists);
            }
        }

        var serviceOrderToCreate = new ServiceOrder
        {
            VehicleId = vehicleId,
            OrderStatusId = orderStatusId,
            EntryDate = entryDate,
            EstimatedDeliveryDate = estimatedDeliveryDate,
            GeneralDescription = generalDescription,
            CancellationReason = null,
            CancellationDate = null
        };

        var createRepository = _unitOfWork.Repository<ServiceOrder>();
        await createRepository.AddAsync(serviceOrderToCreate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceOrderDto>.Success(MapToDto(serviceOrderToCreate));
    }

    public async Task<Result<ServiceOrderDto>> UpdateAsync(
        int id,
        UpdateServiceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(id, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.NotFound);
        }

        var vehicleId = request?.VehicleId ?? 0;
        var orderStatusId = request?.OrderStatusId ?? 0;
        var entryDate = request?.EntryDate ?? default;
        var estimatedDeliveryDate = request?.EstimatedDeliveryDate;
        var generalDescription = NormalizeOptionalText(request?.GeneralDescription);
        var cancellationReason = NormalizeOptionalText(request?.CancellationReason);
        var cancellationDate = request?.CancellationDate;

        var validationError = Validate(vehicleId, orderStatusId, entryDate, estimatedDeliveryDate);
        if (validationError is not null)
        {
            return Result<ServiceOrderDto>.Failure(validationError);
        }

        var vehicleRepository = _unitOfWork.Repository<Vehicle>();
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.VehicleNotFound);
        }

        if (!vehicle.IsActive)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.VehicleInactive);
        }

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatus = await orderStatusRepository.GetByIdAsync(orderStatusId, cancellationToken);

        if (orderStatus is null)
        {
            return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.OrderStatusNotFound);
        }

        var activeOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { PendingStatusName, InProgressStatusName },
            cancellationToken);

        if (activeOrderStatusIds.Contains(orderStatusId))
        {
            var activeOrderAlreadyExists = await serviceOrderRepository.ExistsAsync(
                x => x.VehicleId == vehicleId && activeOrderStatusIds.Contains(x.OrderStatusId) && x.ServiceOrderId != id,
                cancellationToken);

            if (activeOrderAlreadyExists)
            {
                return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.ActiveOrderAlreadyExists);
            }
        }

        if (IsCancelledOrVoidedStatus(orderStatus.Name))
        {
            if (string.IsNullOrWhiteSpace(cancellationReason))
            {
                return Result<ServiceOrderDto>.Failure(ServiceOrderErrors.CancellationReasonRequired);
            }

            if (!cancellationDate.HasValue || cancellationDate.Value == default)
            {
                cancellationDate = DateTime.UtcNow;
            }
        }
        else
        {
            cancellationReason = null;
            cancellationDate = null;
        }

        serviceOrder.VehicleId = vehicleId;
        serviceOrder.OrderStatusId = orderStatusId;
        serviceOrder.EntryDate = entryDate;
        serviceOrder.EstimatedDeliveryDate = estimatedDeliveryDate;
        serviceOrder.GeneralDescription = generalDescription;
        serviceOrder.CancellationReason = cancellationReason;
        serviceOrder.CancellationDate = cancellationDate;

        serviceOrderRepository.Update(serviceOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceOrderDto>.Success(MapToDto(serviceOrder));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(id, cancellationToken);

        if (serviceOrder is null)
        {
            return Result.Failure(ServiceOrderErrors.NotFound);
        }

        var vehicleEntryInventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var inUseByVehicleEntryInventory = await vehicleEntryInventoryRepository.ExistsAsync(
            x => x.ServiceOrderId == id,
            cancellationToken);

        if (inUseByVehicleEntryInventory)
        {
            return Result.Failure(ServiceOrderErrors.InUse);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var inUseByOrderService = await orderServiceRepository.ExistsAsync(
            x => x.ServiceOrderId == id,
            cancellationToken);

        if (inUseByOrderService)
        {
            return Result.Failure(ServiceOrderErrors.InUse);
        }

        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var inUseByOrderStatusHistory = await orderStatusHistoryRepository.ExistsAsync(
            x => x.ServiceOrderId == id,
            cancellationToken);

        if (inUseByOrderStatusHistory)
        {
            return Result.Failure(ServiceOrderErrors.InUse);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var inUseByInvoice = await invoiceRepository.ExistsAsync(
            x => x.ServiceOrderId == id,
            cancellationToken);

        if (inUseByInvoice)
        {
            return Result.Failure(ServiceOrderErrors.InUse);
        }

        serviceOrderRepository.Remove(serviceOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ServiceOrderDto MapToDto(ServiceOrder serviceOrder)
    {
        return new ServiceOrderDto
        {
            ServiceOrderId = serviceOrder.ServiceOrderId,
            VehicleId = serviceOrder.VehicleId,
            OrderStatusId = serviceOrder.OrderStatusId,
            EntryDate = serviceOrder.EntryDate,
            EstimatedDeliveryDate = serviceOrder.EstimatedDeliveryDate,
            GeneralDescription = serviceOrder.GeneralDescription,
            CancellationReason = serviceOrder.CancellationReason,
            CancellationDate = serviceOrder.CancellationDate,
            CreatedAt = serviceOrder.CreatedAt
        };
    }

    private async Task<int[]> GetOrderStatusIdsByNamesAsync(
        IReadOnlyCollection<string> orderStatusNames,
        CancellationToken cancellationToken)
    {
        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatuses = await orderStatusRepository.GetAllAsync(cancellationToken);
        var orderStatusNameSet = new HashSet<string>(orderStatusNames, StringComparer.OrdinalIgnoreCase);

        return orderStatuses
            .Where(x => orderStatusNameSet.Contains(x.Name))
            .Select(x => x.OrderStatusId)
            .Distinct()
            .ToArray();
    }

    private static DateTime NormalizeEntryDate(DateTime? entryDate)
    {
        return entryDate ?? DateTime.UtcNow;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool IsCancelledOrVoidedStatus(string statusName)
    {
        return statusName.Equals(CancelledStatusName, StringComparison.OrdinalIgnoreCase) ||
               statusName.Equals(VoidedStatusName, StringComparison.OrdinalIgnoreCase);
    }

    private static Error? Validate(
        int vehicleId,
        int orderStatusId,
        DateTime entryDate,
        DateTime? estimatedDeliveryDate)
    {
        if (vehicleId <= 0)
        {
            return ServiceOrderErrors.VehicleIdInvalid;
        }

        if (orderStatusId <= 0)
        {
            return ServiceOrderErrors.OrderStatusIdInvalid;
        }

        if (entryDate == default || entryDate < MinimumEntryDate)
        {
            return ServiceOrderErrors.EntryDateInvalid;
        }

        if (estimatedDeliveryDate.HasValue && estimatedDeliveryDate.Value < entryDate)
        {
            return ServiceOrderErrors.EstimatedDeliveryDateInvalid;
        }

        return null;
    }
}
