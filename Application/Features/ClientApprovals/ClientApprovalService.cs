using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.ClientApprovals.Dtos;
using Application.Features.ClientApprovals.Errors;
using Domain.Entities;

namespace Application.Features.ClientApprovals;

public class ClientApprovalService : IClientApprovalService
{
    private const string ClientRoleName = "Client";
    private const string ServiceApprovalType = "service";
    private const string PartApprovalType = "part";

    private readonly IUnitOfWork _unitOfWork;

    public ClientApprovalService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ClientPendingApprovalDto>>> GetPendingApprovalsAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateClientPersonAsync(currentPersonId, cancellationToken);
        if (validationError is not null)
        {
            return Result<IReadOnlyList<ClientPendingApprovalDto>>.Failure(validationError);
        }

        var ownerships = await GetClientOwnershipsAsync(currentPersonId, cancellationToken);
        if (ownerships.Count == 0)
        {
            return Result<IReadOnlyList<ClientPendingApprovalDto>>.Success(Array.Empty<ClientPendingApprovalDto>());
        }

        var vehicleIds = ownerships
            .Select(x => x.VehicleId)
            .Distinct()
            .ToList();

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var accessibleServiceOrders = serviceOrders
            .Where(x => IsOwnedAtOrderEntry(ownerships, x))
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ServiceOrderId)
            .ToList();

        if (accessibleServiceOrders.Count == 0)
        {
            return Result<IReadOnlyList<ClientPendingApprovalDto>>.Success(Array.Empty<ClientPendingApprovalDto>());
        }

        var serviceOrderIds = accessibleServiceOrders
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToList();

        var orderServices = await _unitOfWork.Repository<OrderService>().FindAsync(
            x => serviceOrderIds.Contains(x.ServiceOrderId),
            cancellationToken);

        var orderServiceIds = orderServices
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToList();

        var pendingParts = orderServiceIds.Count == 0
            ? Array.Empty<OrderServicePart>()
            : (await _unitOfWork.Repository<OrderServicePart>().FindAsync(
                x => orderServiceIds.Contains(x.OrderServiceId) && x.CustomerApproved == null,
                cancellationToken)).ToArray();

        var pendingServices = orderServices
            .Where(x => x.CustomerApproved == null)
            .ToArray();

        if (pendingServices.Length == 0 && pendingParts.Length == 0)
        {
            return Result<IReadOnlyList<ClientPendingApprovalDto>>.Success(Array.Empty<ClientPendingApprovalDto>());
        }

        var vehicles = await _unitOfWork.Repository<Vehicle>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var serviceTypeIds = pendingServices
            .Select(x => x.ServiceTypeId)
            .Distinct()
            .ToList();

        var serviceTypes = serviceTypeIds.Count == 0
            ? Array.Empty<ServiceType>()
            : (await _unitOfWork.Repository<ServiceType>().FindAsync(
                x => serviceTypeIds.Contains(x.ServiceTypeId),
                cancellationToken)).ToArray();

        var partIds = pendingParts
            .Select(x => x.PartId)
            .Distinct()
            .ToList();

        var parts = partIds.Count == 0
            ? Array.Empty<Part>()
            : (await _unitOfWork.Repository<Part>().FindAsync(
                x => partIds.Contains(x.PartId),
                cancellationToken)).ToArray();

        var vehicleById = vehicles.ToDictionary(x => x.VehicleId, x => x);
        var orderServiceById = orderServices.ToDictionary(x => x.OrderServiceId, x => x);
        var serviceTypeById = serviceTypes.ToDictionary(x => x.ServiceTypeId, x => x);
        var partById = parts.ToDictionary(x => x.PartId, x => x);

        var pendingServicesByOrderId = pendingServices
            .GroupBy(x => x.ServiceOrderId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<ClientPendingServiceApprovalDto>)x
                    .OrderBy(y => y.OrderServiceId)
                    .Select(y => MapPendingService(y, serviceTypeById))
                    .ToList());

        var pendingPartsByOrderId = pendingParts
            .Where(x => orderServiceById.ContainsKey(x.OrderServiceId))
            .GroupBy(x => orderServiceById[x.OrderServiceId].ServiceOrderId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<ClientPendingPartApprovalDto>)x
                    .OrderBy(y => y.OrderServiceId)
                    .ThenBy(y => y.OrderServicePartId)
                    .Select(y => MapPendingPart(y, partById))
                    .ToList());

        var result = accessibleServiceOrders
            .Where(x => pendingServicesByOrderId.ContainsKey(x.ServiceOrderId) ||
                        pendingPartsByOrderId.ContainsKey(x.ServiceOrderId))
            .Select(x => new ClientPendingApprovalDto
            {
                ServiceOrderId = x.ServiceOrderId,
                VehicleId = x.VehicleId,
                VehiclePlate = vehicleById.TryGetValue(x.VehicleId, out var vehicle) ? vehicle.Plate : string.Empty,
                OrderStatusId = x.OrderStatusId,
                EntryDate = x.EntryDate,
                GeneralDescription = x.GeneralDescription,
                PendingServices = pendingServicesByOrderId.TryGetValue(x.ServiceOrderId, out var services)
                    ? services
                    : Array.Empty<ClientPendingServiceApprovalDto>(),
                PendingParts = pendingPartsByOrderId.TryGetValue(x.ServiceOrderId, out var orderParts)
                    ? orderParts
                    : Array.Empty<ClientPendingPartApprovalDto>()
            })
            .ToList();

        return Result<IReadOnlyList<ClientPendingApprovalDto>>.Success(result);
    }

    public async Task<Result<ClientApprovalActionResultDto>> ApproveOrderServiceAsync(
        int orderServiceId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServiceApprovalAsync(orderServiceId, currentPersonId, approve: true, cancellationToken);
    }

    public async Task<Result<ClientApprovalActionResultDto>> RejectOrderServiceAsync(
        int orderServiceId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServiceApprovalAsync(orderServiceId, currentPersonId, approve: false, cancellationToken);
    }

    public async Task<Result<ClientApprovalActionResultDto>> ApproveOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServicePartApprovalAsync(orderServicePartId, currentPersonId, approve: true, cancellationToken);
    }

    public async Task<Result<ClientApprovalActionResultDto>> RejectOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServicePartApprovalAsync(orderServicePartId, currentPersonId, approve: false, cancellationToken);
    }

    private async Task<Result<ClientApprovalActionResultDto>> SetOrderServiceApprovalAsync(
        int orderServiceId,
        int currentPersonId,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (orderServiceId <= 0)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.OrderServiceIdInvalid);
        }

        var validationError = await ValidateClientPersonAsync(currentPersonId, cancellationToken);
        if (validationError is not null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(validationError);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.ServiceOrderNotFound);
        }

        var ownsOrder = await ClientOwnsServiceOrderAsync(serviceOrder, currentPersonId, cancellationToken);
        if (!ownsOrder)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.NotOwner);
        }

        var decidedError = GetAlreadyDecidedError(orderService.CustomerApproved, approve);
        if (decidedError is not null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(decidedError);
        }

        var approvalDate = DateTime.UtcNow;
        orderService.CustomerApproved = approve;
        orderService.ApprovalDate = approvalDate;

        orderServiceRepository.Update(orderService);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ClientApprovalActionResultDto>.Success(new ClientApprovalActionResultDto
        {
            Id = orderService.OrderServiceId,
            Type = ServiceApprovalType,
            CustomerApproved = approve,
            ApprovalDate = approvalDate,
            ServiceOrderId = serviceOrder.ServiceOrderId
        });
    }

    private async Task<Result<ClientApprovalActionResultDto>> SetOrderServicePartApprovalAsync(
        int orderServicePartId,
        int currentPersonId,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (orderServicePartId <= 0)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.OrderServicePartIdInvalid);
        }

        var validationError = await ValidateClientPersonAsync(currentPersonId, cancellationToken);
        if (validationError is not null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(validationError);
        }

        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServicePart = await orderServicePartRepository.GetByIdAsync(orderServicePartId, cancellationToken);
        if (orderServicePart is null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.OrderServicePartNotFound);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServicePart.OrderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.ServiceOrderNotFound);
        }

        var ownsOrder = await ClientOwnsServiceOrderAsync(serviceOrder, currentPersonId, cancellationToken);
        if (!ownsOrder)
        {
            return Result<ClientApprovalActionResultDto>.Failure(ClientApprovalErrors.NotOwner);
        }

        var decidedError = GetAlreadyDecidedError(orderServicePart.CustomerApproved, approve);
        if (decidedError is not null)
        {
            return Result<ClientApprovalActionResultDto>.Failure(decidedError);
        }

        var approvalDate = DateTime.UtcNow;
        orderServicePart.CustomerApproved = approve;
        orderServicePart.ApprovalDate = approvalDate;

        orderServicePartRepository.Update(orderServicePart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ClientApprovalActionResultDto>.Success(new ClientApprovalActionResultDto
        {
            Id = orderServicePart.OrderServicePartId,
            Type = PartApprovalType,
            CustomerApproved = approve,
            ApprovalDate = approvalDate,
            ServiceOrderId = serviceOrder.ServiceOrderId
        });
    }

    private async Task<Error?> ValidateClientPersonAsync(int currentPersonId, CancellationToken cancellationToken)
    {
        if (currentPersonId <= 0)
        {
            return ClientApprovalErrors.InvalidPersonId;
        }

        var personExists = await _unitOfWork.Repository<Person>().ExistsAsync(
            x => x.PersonId == currentPersonId,
            cancellationToken);

        if (!personExists)
        {
            return ClientApprovalErrors.InvalidPersonId;
        }

        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);
        var clientRoleId = roles
            .Where(x => x.RoleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();

        if (!clientRoleId.HasValue)
        {
            return ClientApprovalErrors.NotOwner;
        }

        var hasActiveClientRole = await _unitOfWork.Repository<PersonRole>().ExistsAsync(
            x => x.PersonId == currentPersonId && x.RoleId == clientRoleId.Value && x.IsActive,
            cancellationToken);

        return hasActiveClientRole
            ? null
            : ClientApprovalErrors.NotOwner;
    }

    private async Task<bool> ClientOwnsServiceOrderAsync(
        ServiceOrder serviceOrder,
        int currentPersonId,
        CancellationToken cancellationToken)
    {
        var ownerships = await GetClientOwnershipsAsync(currentPersonId, cancellationToken);
        return IsOwnedAtOrderEntry(ownerships, serviceOrder);
    }

    private async Task<IReadOnlyList<VehicleOwnerHistory>> GetClientOwnershipsAsync(
        int currentPersonId,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<VehicleOwnerHistory>().FindAsync(
            x => x.PersonId == currentPersonId,
            cancellationToken);
    }

    private static bool IsOwnedAtOrderEntry(
        IReadOnlyList<VehicleOwnerHistory> ownerships,
        ServiceOrder serviceOrder)
    {
        return ownerships.Any(x =>
            x.VehicleId == serviceOrder.VehicleId &&
            x.StartDate <= serviceOrder.EntryDate &&
            (x.EndDate is null || x.EndDate >= serviceOrder.EntryDate));
    }

    private static Error? GetAlreadyDecidedError(bool? currentDecision, bool requestedDecision)
    {
        if (currentDecision is null)
        {
            return null;
        }

        if (currentDecision == true && requestedDecision)
        {
            return ClientApprovalErrors.AlreadyApproved;
        }

        if (currentDecision == false && !requestedDecision)
        {
            return ClientApprovalErrors.AlreadyRejected;
        }

        return ClientApprovalErrors.ApprovalAlreadyDecided;
    }

    private static ClientPendingServiceApprovalDto MapPendingService(
        OrderService orderService,
        IReadOnlyDictionary<int, ServiceType> serviceTypeById)
    {
        return new ClientPendingServiceApprovalDto
        {
            OrderServiceId = orderService.OrderServiceId,
            ServiceTypeId = orderService.ServiceTypeId,
            ServiceTypeName = serviceTypeById.TryGetValue(orderService.ServiceTypeId, out var serviceType)
                ? serviceType.Name
                : null,
            Description = orderService.Description,
            LaborCost = orderService.LaborCost,
            WorkPerformed = orderService.WorkPerformed,
            CustomerApproved = orderService.CustomerApproved,
            ApprovalDate = orderService.ApprovalDate
        };
    }

    private static ClientPendingPartApprovalDto MapPendingPart(
        OrderServicePart orderServicePart,
        IReadOnlyDictionary<int, Part> partById)
    {
        return new ClientPendingPartApprovalDto
        {
            OrderServicePartId = orderServicePart.OrderServicePartId,
            OrderServiceId = orderServicePart.OrderServiceId,
            PartId = orderServicePart.PartId,
            PartName = partById.TryGetValue(orderServicePart.PartId, out var part)
                ? part.Description
                : null,
            Quantity = orderServicePart.Quantity,
            AppliedUnitPrice = orderServicePart.AppliedUnitPrice,
            Subtotal = orderServicePart.Quantity * orderServicePart.AppliedUnitPrice,
            CustomerApproved = orderServicePart.CustomerApproved,
            ApprovalDate = orderServicePart.ApprovalDate
        };
    }
}
