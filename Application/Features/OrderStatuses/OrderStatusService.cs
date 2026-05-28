using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.OrderStatuses.Dtos;
using Application.Features.OrderStatuses.Errors;
using Application.Features.OrderStatuses.Requests;
using Domain.Entities;

namespace Application.Features.OrderStatuses;

public class OrderStatusService : IOrderStatusService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<OrderStatusDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatuses = await orderStatusRepository.GetAllAsync(cancellationToken);

        var orderStatusDtos = orderStatuses
            .OrderBy(x => x.OrderStatusId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<OrderStatusDto>>.Success(orderStatusDtos);
    }

    public async Task<Result<OrderStatusDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatus = await orderStatusRepository.GetByIdAsync(id, cancellationToken);

        if (orderStatus is null)
        {
            return Result<OrderStatusDto>.Failure(OrderStatusErrors.NotFound);
        }

        return Result<OrderStatusDto>.Success(MapToDto(orderStatus));
    }

    public async Task<Result<OrderStatusDto>> CreateAsync(
        CreateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<OrderStatusDto>.Failure(validationError);
        }

        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var nameAlreadyExists = await orderStatusRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<OrderStatusDto>.Failure(OrderStatusErrors.NameAlreadyExists);
        }

        var orderStatus = new OrderStatus
        {
            Name = normalizedName
        };

        await orderStatusRepository.AddAsync(orderStatus, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderStatusDto>.Success(MapToDto(orderStatus));
    }

    public async Task<Result<OrderStatusDto>> UpdateAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatus = await orderStatusRepository.GetByIdAsync(id, cancellationToken);

        if (orderStatus is null)
        {
            return Result<OrderStatusDto>.Failure(OrderStatusErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<OrderStatusDto>.Failure(validationError);
        }

        var nameAlreadyExists = await orderStatusRepository.ExistsAsync(
            x => x.Name == normalizedName && x.OrderStatusId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<OrderStatusDto>.Failure(OrderStatusErrors.NameAlreadyExists);
        }

        orderStatus.Name = normalizedName;

        orderStatusRepository.Update(orderStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderStatusDto>.Success(MapToDto(orderStatus));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderStatusRepository = _unitOfWork.Repository<OrderStatus>();
        var orderStatus = await orderStatusRepository.GetByIdAsync(id, cancellationToken);

        if (orderStatus is null)
        {
            return Result.Failure(OrderStatusErrors.NotFound);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var inUseByServiceOrder = await serviceOrderRepository.ExistsAsync(
            x => x.OrderStatusId == id,
            cancellationToken);

        if (inUseByServiceOrder)
        {
            return Result.Failure(OrderStatusErrors.InUse);
        }

        var orderStatusHistoryRepository = _unitOfWork.Repository<OrderStatusHistory>();
        var inUseAsPreviousStatus = await orderStatusHistoryRepository.ExistsAsync(
            x => x.PreviousOrderStatusId == id,
            cancellationToken);

        if (inUseAsPreviousStatus)
        {
            return Result.Failure(OrderStatusErrors.InUse);
        }

        var inUseAsNewStatus = await orderStatusHistoryRepository.ExistsAsync(
            x => x.NewOrderStatusId == id,
            cancellationToken);

        if (inUseAsNewStatus)
        {
            return Result.Failure(OrderStatusErrors.InUse);
        }

        orderStatusRepository.Remove(orderStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static OrderStatusDto MapToDto(OrderStatus orderStatus)
    {
        return new OrderStatusDto
        {
            OrderStatusId = orderStatus.OrderStatusId,
            Name = orderStatus.Name
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
            return OrderStatusErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return OrderStatusErrors.NameTooLong;
        }

        return null;
    }
}
