using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.OrderServiceParts.Dtos;
using Application.Features.OrderServiceParts.Errors;
using Application.Features.OrderServiceParts.Requests;
using Domain.Entities;

namespace Application.Features.OrderServiceParts;

public class OrderServicePartService : IOrderServicePartService
{
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";

    private readonly IUnitOfWork _unitOfWork;

    public OrderServicePartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<OrderServicePartDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServiceParts = await orderServicePartRepository.GetAllAsync(cancellationToken);

        var orderServicePartDtos = orderServiceParts
            .OrderBy(x => x.OrderServicePartId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<OrderServicePartDto>>.Success(orderServicePartDtos);
    }

    public async Task<Result<OrderServicePartDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServicePart = await orderServicePartRepository.GetByIdAsync(id, cancellationToken);

        if (orderServicePart is null)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.NotFound);
        }

        return Result<OrderServicePartDto>.Success(MapToDto(orderServicePart));
    }

    public async Task<Result<OrderServicePartDto>> CreateAsync(
        CreateOrderServicePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderServiceId = request?.OrderServiceId ?? 0;
        var partId = request?.PartId ?? 0;
        var quantity = request?.Quantity ?? 0;
        var appliedUnitPrice = request?.AppliedUnitPrice ?? 0m;
        var customerApproved = request?.CustomerApproved;
        var approvalDate = ResolveApprovalDate(customerApproved, request?.ApprovalDate);

        var validationError = Validate(orderServiceId, partId, quantity, appliedUnitPrice);
        if (validationError is not null)
        {
            return Result<OrderServicePartDto>.Failure(validationError);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.OrderServiceNotFound);
        }

        var canModifyOrderService = await CanModifyOrderServiceAsync(orderService, cancellationToken);
        if (!canModifyOrderService)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.OrderServiceCannotBeModifiedConflict);
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(partId, cancellationToken);
        if (part is null)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.PartNotFound);
        }

        if (!part.IsActive)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.PartInactive);
        }

        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var duplicatePartForOrderService = await orderServicePartRepository.ExistsAsync(
            x => x.OrderServiceId == orderServiceId && x.PartId == partId,
            cancellationToken);

        if (duplicatePartForOrderService)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.DuplicatePartForOrderServiceConflict);
        }

        if (IsApproved(customerApproved))
        {
            if (part.Stock < quantity)
            {
                return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.InsufficientStockConflict);
            }

            part.Stock -= quantity;
            partRepository.Update(part);
        }

        var orderServicePart = new OrderServicePart
        {
            OrderServiceId = orderServiceId,
            PartId = partId,
            Quantity = quantity,
            AppliedUnitPrice = appliedUnitPrice,
            CustomerApproved = customerApproved,
            ApprovalDate = approvalDate
        };

        await orderServicePartRepository.AddAsync(orderServicePart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderServicePartDto>.Success(MapToDto(orderServicePart));
    }

    public async Task<Result<OrderServicePartDto>> UpdateAsync(
        int id,
        UpdateOrderServicePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServicePart = await orderServicePartRepository.GetByIdAsync(id, cancellationToken);
        if (orderServicePart is null)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.NotFound);
        }

        var newOrderServiceId = request?.OrderServiceId ?? 0;
        var newPartId = request?.PartId ?? 0;
        var newQuantity = request?.Quantity ?? 0;
        var newAppliedUnitPrice = request?.AppliedUnitPrice ?? 0m;
        var newCustomerApproved = request?.CustomerApproved;
        var newApprovalDate = ResolveApprovalDate(newCustomerApproved, request?.ApprovalDate);

        var validationError = Validate(newOrderServiceId, newPartId, newQuantity, newAppliedUnitPrice);
        if (validationError is not null)
        {
            return Result<OrderServicePartDto>.Failure(validationError);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var newOrderService = await orderServiceRepository.GetByIdAsync(newOrderServiceId, cancellationToken);
        if (newOrderService is null)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.OrderServiceNotFound);
        }

        var canModifyOrderService = await CanModifyOrderServiceAsync(newOrderService, cancellationToken);
        if (!canModifyOrderService)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.OrderServiceCannotBeModifiedConflict);
        }

        var partRepository = _unitOfWork.Repository<Part>();

        var oldPart = await partRepository.GetByIdAsync(orderServicePart.PartId, cancellationToken);
        if (oldPart is null)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.PartNotFound);
        }

        Part newPart;
        if (newPartId == orderServicePart.PartId)
        {
            newPart = oldPart;
        }
        else
        {
            var loadedNewPart = await partRepository.GetByIdAsync(newPartId, cancellationToken);
            if (loadedNewPart is null)
            {
                return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.PartNotFound);
            }

            newPart = loadedNewPart;
        }

        if (!newPart.IsActive)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.PartInactive);
        }

        var duplicatePartForOrderService = await orderServicePartRepository.ExistsAsync(
            x => x.OrderServiceId == newOrderServiceId && x.PartId == newPartId && x.OrderServicePartId != id,
            cancellationToken);

        if (duplicatePartForOrderService)
        {
            return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.DuplicatePartForOrderServiceConflict);
        }

        var oldApproved = IsApproved(orderServicePart.CustomerApproved);
        var newApproved = IsApproved(newCustomerApproved);
        var oldPartId = orderServicePart.PartId;
        var oldQuantity = orderServicePart.Quantity;

        var oldPartStockChanged = false;
        var newPartStockChanged = false;

        if (!oldApproved && !newApproved)
        {
            // No stock change.
        }
        else if (!oldApproved && newApproved)
        {
            if (newPart.Stock < newQuantity)
            {
                return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.InsufficientStockConflict);
            }

            newPart.Stock -= newQuantity;
            if (newPartId == oldPartId)
            {
                oldPartStockChanged = true;
            }
            else
            {
                newPartStockChanged = true;
            }
        }
        else if (oldApproved && !newApproved)
        {
            oldPart.Stock += oldQuantity;
            oldPartStockChanged = true;
        }
        else if (newPartId == oldPartId)
        {
            var stockAfterAdjustment = oldPart.Stock + oldQuantity - newQuantity;
            if (stockAfterAdjustment < 0)
            {
                return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.StockWouldBeNegativeInvalid);
            }

            oldPart.Stock = stockAfterAdjustment;
            oldPartStockChanged = true;
        }
        else
        {
            oldPart.Stock += oldQuantity;
            oldPartStockChanged = true;

            if (newPart.Stock < newQuantity)
            {
                return Result<OrderServicePartDto>.Failure(OrderServicePartErrors.InsufficientStockConflict);
            }

            newPart.Stock -= newQuantity;
            newPartStockChanged = true;
        }

        orderServicePart.OrderServiceId = newOrderServiceId;
        orderServicePart.PartId = newPartId;
        orderServicePart.Quantity = newQuantity;
        orderServicePart.AppliedUnitPrice = newAppliedUnitPrice;
        orderServicePart.CustomerApproved = newCustomerApproved;
        orderServicePart.ApprovalDate = newApprovalDate;

        orderServicePartRepository.Update(orderServicePart);

        if (oldPartStockChanged)
        {
            partRepository.Update(oldPart);
        }

        if (newPartStockChanged && newPartId != oldPartId)
        {
            partRepository.Update(newPart);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderServicePartDto>.Success(MapToDto(orderServicePart));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServicePart = await orderServicePartRepository.GetByIdAsync(id, cancellationToken);
        if (orderServicePart is null)
        {
            return Result.Failure(OrderServicePartErrors.NotFound);
        }

        if (IsApproved(orderServicePart.CustomerApproved))
        {
            var partRepository = _unitOfWork.Repository<Part>();
            var part = await partRepository.GetByIdAsync(orderServicePart.PartId, cancellationToken);
            if (part is null)
            {
                return Result.Failure(OrderServicePartErrors.PartNotFound);
            }

            part.Stock += orderServicePart.Quantity;
            partRepository.Update(part);
        }

        orderServicePartRepository.Remove(orderServicePart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static OrderServicePartDto MapToDto(OrderServicePart orderServicePart)
    {
        return new OrderServicePartDto
        {
            OrderServicePartId = orderServicePart.OrderServicePartId,
            OrderServiceId = orderServicePart.OrderServiceId,
            PartId = orderServicePart.PartId,
            Quantity = orderServicePart.Quantity,
            AppliedUnitPrice = orderServicePart.AppliedUnitPrice,
            Subtotal = CalculateSubtotal(orderServicePart.Quantity, orderServicePart.AppliedUnitPrice),
            CustomerApproved = orderServicePart.CustomerApproved,
            ApprovalDate = orderServicePart.ApprovalDate
        };
    }

    private static Error? Validate(int orderServiceId, int partId, int quantity, decimal appliedUnitPrice)
    {
        if (orderServiceId <= 0)
        {
            return OrderServicePartErrors.OrderServiceIdInvalid;
        }

        if (partId <= 0)
        {
            return OrderServicePartErrors.PartIdInvalid;
        }

        if (quantity <= 0)
        {
            return OrderServicePartErrors.QuantityInvalid;
        }

        if (appliedUnitPrice < 0m)
        {
            return OrderServicePartErrors.AppliedUnitPriceInvalid;
        }

        return null;
    }

    private async Task<bool> CanModifyOrderServiceAsync(OrderService orderService, CancellationToken cancellationToken)
    {
        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return false;
        }

        var blockedOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName },
            cancellationToken);

        return !blockedOrderStatusIds.Contains(serviceOrder.OrderStatusId);
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

    private static DateTime? ResolveApprovalDate(bool? customerApproved, DateTime? approvalDate)
    {
        if (!customerApproved.HasValue)
        {
            return null;
        }

        if (!approvalDate.HasValue || approvalDate.Value == default)
        {
            return DateTime.UtcNow;
        }

        return approvalDate;
    }

    private static bool IsApproved(bool? customerApproved)
    {
        return customerApproved == true;
    }

    private static decimal CalculateSubtotal(int quantity, decimal appliedUnitPrice)
    {
        return quantity * appliedUnitPrice;
    }
}
