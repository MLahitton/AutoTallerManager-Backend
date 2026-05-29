using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.OrderServices.Dtos;
using Application.Features.OrderServices.Errors;
using Application.Features.OrderServices.Requests;
using Domain.Entities;

namespace Application.Features.OrderServices;

public class OrderServiceService : IOrderServiceService
{
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";

    private readonly IUnitOfWork _unitOfWork;

    public OrderServiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<OrderServiceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderServices = await orderServiceRepository.GetAllAsync(cancellationToken);

        var orderServiceDtos = orderServices
            .OrderBy(x => x.OrderServiceId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<OrderServiceDto>>.Success(orderServiceDtos);
    }

    public async Task<Result<OrderServiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(id, cancellationToken);

        if (orderService is null)
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.NotFound);
        }

        return Result<OrderServiceDto>.Success(MapToDto(orderService));
    }

    public async Task<Result<OrderServiceDto>> CreateAsync(
        CreateOrderServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var serviceTypeId = request?.ServiceTypeId ?? 0;
        var description = NormalizeOptionalText(request?.Description);
        var workPerformed = NormalizeOptionalText(request?.WorkPerformed);
        var laborCost = request?.LaborCost ?? 0m;
        var customerApproved = request?.CustomerApproved;
        var approvalDate = request?.ApprovalDate;

        var validationError = Validate(serviceOrderId, serviceTypeId, laborCost);
        if (validationError is not null)
        {
            return Result<OrderServiceDto>.Failure(validationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.ServiceOrderNotFound);
        }

        var blockedOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName },
            cancellationToken);

        if (blockedOrderStatusIds.Contains(serviceOrder.OrderStatusId))
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.ServiceOrderCannotBeModifiedConflict);
        }

        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceTypeExists = await serviceTypeRepository.ExistsAsync(
            x => x.ServiceTypeId == serviceTypeId,
            cancellationToken);

        if (!serviceTypeExists)
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.ServiceTypeNotFound);
        }

        approvalDate = ResolveApprovalDate(customerApproved, approvalDate);

        var orderService = new OrderService
        {
            ServiceOrderId = serviceOrderId,
            ServiceTypeId = serviceTypeId,
            Description = description,
            WorkPerformed = workPerformed,
            LaborCost = laborCost,
            CustomerApproved = customerApproved,
            ApprovalDate = approvalDate
        };

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        await orderServiceRepository.AddAsync(orderService, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderServiceDto>.Success(MapToDto(orderService));
    }

    public async Task<Result<OrderServiceDto>> UpdateAsync(
        int id,
        UpdateOrderServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(id, cancellationToken);

        if (orderService is null)
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.NotFound);
        }

        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var serviceTypeId = request?.ServiceTypeId ?? 0;
        var description = NormalizeOptionalText(request?.Description);
        var workPerformed = NormalizeOptionalText(request?.WorkPerformed);
        var laborCost = request?.LaborCost ?? 0m;
        var customerApproved = request?.CustomerApproved;
        var approvalDate = request?.ApprovalDate;

        var validationError = Validate(serviceOrderId, serviceTypeId, laborCost);
        if (validationError is not null)
        {
            return Result<OrderServiceDto>.Failure(validationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.ServiceOrderNotFound);
        }

        var blockedOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName },
            cancellationToken);

        if (blockedOrderStatusIds.Contains(serviceOrder.OrderStatusId))
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.ServiceOrderCannotBeModifiedConflict);
        }

        var serviceTypeRepository = _unitOfWork.Repository<ServiceType>();
        var serviceTypeExists = await serviceTypeRepository.ExistsAsync(
            x => x.ServiceTypeId == serviceTypeId,
            cancellationToken);

        if (!serviceTypeExists)
        {
            return Result<OrderServiceDto>.Failure(OrderServiceErrors.ServiceTypeNotFound);
        }

        approvalDate = ResolveApprovalDate(customerApproved, approvalDate);

        orderService.ServiceOrderId = serviceOrderId;
        orderService.ServiceTypeId = serviceTypeId;
        orderService.Description = description;
        orderService.WorkPerformed = workPerformed;
        orderService.LaborCost = laborCost;
        orderService.CustomerApproved = customerApproved;
        orderService.ApprovalDate = approvalDate;

        orderServiceRepository.Update(orderService);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderServiceDto>.Success(MapToDto(orderService));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(id, cancellationToken);

        if (orderService is null)
        {
            return Result.Failure(OrderServiceErrors.NotFound);
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var inUseByMechanicAssignment = await mechanicAssignmentRepository.ExistsAsync(
            x => x.OrderServiceId == id,
            cancellationToken);

        if (inUseByMechanicAssignment)
        {
            return Result.Failure(OrderServiceErrors.InUse);
        }

        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var inUseByOrderServicePart = await orderServicePartRepository.ExistsAsync(
            x => x.OrderServiceId == id,
            cancellationToken);

        if (inUseByOrderServicePart)
        {
            return Result.Failure(OrderServiceErrors.InUse);
        }

        orderServiceRepository.Remove(orderService);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static OrderServiceDto MapToDto(OrderService orderService)
    {
        return new OrderServiceDto
        {
            OrderServiceId = orderService.OrderServiceId,
            ServiceOrderId = orderService.ServiceOrderId,
            ServiceTypeId = orderService.ServiceTypeId,
            Description = orderService.Description,
            WorkPerformed = orderService.WorkPerformed,
            LaborCost = orderService.LaborCost,
            CustomerApproved = orderService.CustomerApproved,
            ApprovalDate = orderService.ApprovalDate
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

    private static Error? Validate(int serviceOrderId, int serviceTypeId, decimal laborCost)
    {
        if (serviceOrderId <= 0)
        {
            return OrderServiceErrors.ServiceOrderIdInvalid;
        }

        if (serviceTypeId <= 0)
        {
            return OrderServiceErrors.ServiceTypeIdInvalid;
        }

        if (laborCost < 0m)
        {
            return OrderServiceErrors.LaborCostInvalid;
        }

        return null;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
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
}
