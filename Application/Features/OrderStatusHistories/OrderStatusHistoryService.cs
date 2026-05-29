using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.OrderStatusHistories.Dtos;
using Application.Features.OrderStatusHistories.Errors;
using Application.Features.OrderStatusHistories.Requests;
using Domain.Entities;

namespace Application.Features.OrderStatusHistories;

public class OrderStatusHistoryService : IOrderStatusHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusHistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<OrderStatusHistoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var orderStatusHistories = await orderStatusHistoryRepository.GetAllAsync(cancellationToken);

        var orderStatusHistoryDtos = orderStatusHistories
            .OrderBy(x => x.OrderStatusHistoryId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<OrderStatusHistoryDto>>.Success(orderStatusHistoryDtos);
    }

    public async Task<Result<OrderStatusHistoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var orderStatusHistory = await orderStatusHistoryRepository.GetByIdAsync(id, cancellationToken);

        if (orderStatusHistory is null)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.NotFound);
        }

        return Result<OrderStatusHistoryDto>.Success(MapToDto(orderStatusHistory));
    }

    public async Task<Result<OrderStatusHistoryDto>> CreateAsync(
        CreateOrderStatusHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var previousOrderStatusId = request?.PreviousOrderStatusId;
        var newOrderStatusId = request?.NewOrderStatusId ?? 0;
        var changedByUserId = request?.ChangedByUserId ?? 0;
        var observation = NormalizeOptionalText(request?.Observation);

        var validationError = Validate(serviceOrderId, previousOrderStatusId, newOrderStatusId, changedByUserId);
        if (validationError is not null)
        {
            return Result<OrderStatusHistoryDto>.Failure(validationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.ServiceOrderNotFound);
        }

        var currentOrderStatusId = serviceOrder.OrderStatusId;

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();

        int effectivePreviousOrderStatusId;
        if (previousOrderStatusId.HasValue)
        {
            var previousOrderStatusExists = await orderStatusRepository.ExistsAsync(
                x => x.OrderStatusId == previousOrderStatusId.Value,
                cancellationToken);

            if (!previousOrderStatusExists)
            {
                return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.PreviousOrderStatusNotFound);
            }

            if (previousOrderStatusId.Value != currentOrderStatusId)
            {
                return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.PreviousOrderStatusDoesNotMatchCurrentConflict);
            }

            effectivePreviousOrderStatusId = previousOrderStatusId.Value;
        }
        else
        {
            effectivePreviousOrderStatusId = currentOrderStatusId;
        }

        var newOrderStatusExists = await orderStatusRepository.ExistsAsync(
            x => x.OrderStatusId == newOrderStatusId,
            cancellationToken);

        if (!newOrderStatusExists)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.NewOrderStatusNotFound);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var changedByUserExists = await userRepository.ExistsAsync(
            x => x.UserId == changedByUserId,
            cancellationToken);

        if (!changedByUserExists)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.ChangedByUserNotFound);
        }

        var orderStatusHistory = new OrderStatusHistory
        {
            ServiceOrderId = serviceOrderId,
            PreviousOrderStatusId = effectivePreviousOrderStatusId,
            NewOrderStatusId = newOrderStatusId,
            ChangedByUserId = changedByUserId,
            Observation = observation,
            ChangedAt = DateTime.UtcNow
        };

        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        await orderStatusHistoryRepository.AddAsync(orderStatusHistory, cancellationToken);

        serviceOrder.OrderStatusId = newOrderStatusId;
        serviceOrderRepository.Update(serviceOrder);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderStatusHistoryDto>.Success(MapToDto(orderStatusHistory));
    }

    public async Task<Result<OrderStatusHistoryDto>> UpdateAsync(
        int id,
        UpdateOrderStatusHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var orderStatusHistory = await orderStatusHistoryRepository.GetByIdAsync(id, cancellationToken);

        if (orderStatusHistory is null)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.NotFound);
        }

        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var previousOrderStatusId = request?.PreviousOrderStatusId;
        var newOrderStatusId = request?.NewOrderStatusId ?? 0;
        var changedByUserId = request?.ChangedByUserId ?? 0;
        var observation = NormalizeOptionalText(request?.Observation);

        var validationError = Validate(serviceOrderId, previousOrderStatusId, newOrderStatusId, changedByUserId);
        if (validationError is not null)
        {
            return Result<OrderStatusHistoryDto>.Failure(validationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrderExists = await serviceOrderRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (!serviceOrderExists)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.ServiceOrderNotFound);
        }

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();

        if (previousOrderStatusId.HasValue)
        {
            var previousOrderStatusExists = await orderStatusRepository.ExistsAsync(
                x => x.OrderStatusId == previousOrderStatusId.Value,
                cancellationToken);

            if (!previousOrderStatusExists)
            {
                return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.PreviousOrderStatusNotFound);
            }
        }

        var newOrderStatusExists = await orderStatusRepository.ExistsAsync(
            x => x.OrderStatusId == newOrderStatusId,
            cancellationToken);

        if (!newOrderStatusExists)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.NewOrderStatusNotFound);
        }

        var userRepository = _unitOfWork.Repository<User>();
        var changedByUserExists = await userRepository.ExistsAsync(
            x => x.UserId == changedByUserId,
            cancellationToken);

        if (!changedByUserExists)
        {
            return Result<OrderStatusHistoryDto>.Failure(OrderStatusHistoryErrors.ChangedByUserNotFound);
        }

        orderStatusHistory.ServiceOrderId = serviceOrderId;
        orderStatusHistory.PreviousOrderStatusId = previousOrderStatusId;
        orderStatusHistory.NewOrderStatusId = newOrderStatusId;
        orderStatusHistory.ChangedByUserId = changedByUserId;
        orderStatusHistory.Observation = observation;

        orderStatusHistoryRepository.Update(orderStatusHistory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderStatusHistoryDto>.Success(MapToDto(orderStatusHistory));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var orderStatusHistory = await orderStatusHistoryRepository.GetByIdAsync(id, cancellationToken);

        if (orderStatusHistory is null)
        {
            return Result.Failure(OrderStatusHistoryErrors.NotFound);
        }

        orderStatusHistoryRepository.Remove(orderStatusHistory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static OrderStatusHistoryDto MapToDto(OrderStatusHistory orderStatusHistory)
    {
        return new OrderStatusHistoryDto
        {
            OrderStatusHistoryId = orderStatusHistory.OrderStatusHistoryId,
            ServiceOrderId = orderStatusHistory.ServiceOrderId,
            PreviousOrderStatusId = orderStatusHistory.PreviousOrderStatusId,
            NewOrderStatusId = orderStatusHistory.NewOrderStatusId,
            ChangedByUserId = orderStatusHistory.ChangedByUserId,
            Observation = orderStatusHistory.Observation,
            ChangedAt = orderStatusHistory.ChangedAt
        };
    }

    private static Error? Validate(
        int serviceOrderId,
        int? previousOrderStatusId,
        int newOrderStatusId,
        int changedByUserId)
    {
        if (serviceOrderId <= 0)
        {
            return OrderStatusHistoryErrors.ServiceOrderIdInvalid;
        }

        if (previousOrderStatusId.HasValue && previousOrderStatusId.Value <= 0)
        {
            return OrderStatusHistoryErrors.PreviousOrderStatusIdInvalid;
        }

        if (newOrderStatusId <= 0)
        {
            return OrderStatusHistoryErrors.NewOrderStatusIdInvalid;
        }

        if (changedByUserId <= 0)
        {
            return OrderStatusHistoryErrors.ChangedByUserIdInvalid;
        }

        return null;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
