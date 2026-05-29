using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.ServiceExecution.Dtos;
using Application.Features.ServiceExecution.Errors;
using Application.Features.ServiceExecution.Requests;
using Domain.Entities;

namespace Application.Features.ServiceExecution;

public class ServiceExecutionService : IServiceExecutionService
{
    private const string AdminRoleName = "Admin";
    private const string MechanicRoleName = "Mechanic";
    private const string ClientRoleName = "Client";

    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";
    private const string CompletedStatusName = "Completed";

    private readonly IUnitOfWork _unitOfWork;

    public ServiceExecutionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<MechanicAssignedServiceDto>>> GetMyAssignedServicesAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        if (currentPersonId <= 0)
        {
            return Result<IReadOnlyList<MechanicAssignedServiceDto>>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var mechanicAssignments = await _unitOfWork.Repository<MechanicAssignment>().FindAsync(
            x => x.MechanicPersonId == currentPersonId,
            cancellationToken);

        if (mechanicAssignments.Count == 0)
        {
            return Result<IReadOnlyList<MechanicAssignedServiceDto>>.Success(Array.Empty<MechanicAssignedServiceDto>());
        }

        var orderServiceIds = mechanicAssignments
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToList();

        var orderServices = await _unitOfWork.Repository<OrderService>().FindAsync(
            x => orderServiceIds.Contains(x.OrderServiceId),
            cancellationToken);

        var serviceOrderIds = orderServices
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToList();

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => serviceOrderIds.Contains(x.ServiceOrderId),
            cancellationToken);

        var orderServiceById = orderServices.ToDictionary(x => x.OrderServiceId, x => x);
        var serviceOrderById = serviceOrders.ToDictionary(x => x.ServiceOrderId, x => x);

        var result = mechanicAssignments
            .Where(x => orderServiceById.ContainsKey(x.OrderServiceId))
            .Select(x =>
            {
                var orderService = orderServiceById[x.OrderServiceId];
                return new { Assignment = x, OrderService = orderService };
            })
            .Where(x => serviceOrderById.ContainsKey(x.OrderService.ServiceOrderId))
            .OrderBy(x => x.OrderService.ServiceOrderId)
            .ThenBy(x => x.OrderService.OrderServiceId)
            .ThenBy(x => x.Assignment.MechanicAssignmentId)
            .Select(x =>
            {
                var serviceOrder = serviceOrderById[x.OrderService.ServiceOrderId];
                return new MechanicAssignedServiceDto
                {
                    OrderServiceId = x.OrderService.OrderServiceId,
                    ServiceOrderId = x.OrderService.ServiceOrderId,
                    VehicleId = serviceOrder.VehicleId,
                    ServiceTypeId = x.OrderService.ServiceTypeId,
                    Description = x.OrderService.Description,
                    WorkPerformed = x.OrderService.WorkPerformed,
                    LaborCost = x.OrderService.LaborCost,
                    CustomerApproved = x.OrderService.CustomerApproved,
                    ApprovalDate = x.OrderService.ApprovalDate,
                    SpecialtyId = x.Assignment.SpecialtyId
                };
            })
            .ToList();

        return Result<IReadOnlyList<MechanicAssignedServiceDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<MechanicActiveOrderDto>>> GetMyActiveOrdersAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        if (currentPersonId <= 0)
        {
            return Result<IReadOnlyList<MechanicActiveOrderDto>>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var mechanicAssignments = await _unitOfWork.Repository<MechanicAssignment>().FindAsync(
            x => x.MechanicPersonId == currentPersonId,
            cancellationToken);

        if (mechanicAssignments.Count == 0)
        {
            return Result<IReadOnlyList<MechanicActiveOrderDto>>.Success(Array.Empty<MechanicActiveOrderDto>());
        }

        var orderServiceIds = mechanicAssignments
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToList();

        var orderServices = await _unitOfWork.Repository<OrderService>().FindAsync(
            x => orderServiceIds.Contains(x.OrderServiceId),
            cancellationToken);

        var serviceOrderIds = orderServices
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToList();

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => serviceOrderIds.Contains(x.ServiceOrderId),
            cancellationToken);

        var blockedOrderStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName, CompletedStatusName },
            cancellationToken);

        var result = serviceOrders
            .Where(x => !blockedOrderStatusIds.Contains(x.OrderStatusId))
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ServiceOrderId)
            .Select(x => new MechanicActiveOrderDto
            {
                ServiceOrderId = x.ServiceOrderId,
                VehicleId = x.VehicleId,
                OrderStatusId = x.OrderStatusId,
                EntryDate = x.EntryDate,
                EstimatedDeliveryDate = x.EstimatedDeliveryDate,
                GeneralDescription = x.GeneralDescription
            })
            .ToList();

        return Result<IReadOnlyList<MechanicActiveOrderDto>>.Success(result);
    }

    public async Task<Result<ServiceExecutionResultDto>> UpdateWorkPerformedAsync(
        int orderServiceId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        UpdateWorkPerformedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (orderServiceId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceIdInvalid);
        }

        if (currentPersonId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var workPerformed = NormalizeOptionalText(request?.WorkPerformed);
        var laborCost = request?.LaborCost ?? 0m;

        if (string.IsNullOrWhiteSpace(workPerformed))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.WorkPerformedRequired);
        }

        if (laborCost < 0m)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.LaborCostInvalid);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        if (!HasRole(currentRoles, AdminRoleName))
        {
            var assigned = await IsMechanicAssignedToOrderServiceAsync(orderServiceId, currentPersonId, cancellationToken);
            if (!assigned)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicNotAssignedConflict);
            }
        }

        orderService.WorkPerformed = workPerformed;
        orderService.LaborCost = laborCost;

        _unitOfWork.Repository<OrderService>().Update(orderService);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServiceId, "OrderService", "UpdateWorkReport"));
    }

    public async Task<Result<ServiceExecutionResultDto>> AssignMechanicAsync(
        int orderServiceId,
        AssignMechanicRequest request,
        CancellationToken cancellationToken = default)
    {
        if (orderServiceId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceIdInvalid);
        }

        var mechanicPersonId = request?.MechanicPersonId ?? 0;
        var specialtyId = request?.SpecialtyId ?? 0;

        if (mechanicPersonId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicPersonIdInvalid);
        }

        if (specialtyId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.SpecialtyIdInvalid);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        var personExists = await _unitOfWork.Repository<Person>().ExistsAsync(
            x => x.PersonId == mechanicPersonId,
            cancellationToken);
        if (!personExists)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicPersonNotFound);
        }

        var mechanicRoleId = await GetRoleIdByNameAsync(MechanicRoleName, cancellationToken);
        if (!mechanicRoleId.HasValue)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PersonIsNotMechanicInvalid);
        }

        var hasActiveMechanicRole = await _unitOfWork.Repository<PersonRole>().ExistsAsync(
            x => x.PersonId == mechanicPersonId && x.RoleId == mechanicRoleId.Value && x.IsActive,
            cancellationToken);
        if (!hasActiveMechanicRole)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PersonIsNotMechanicInvalid);
        }

        var specialtyExists = await _unitOfWork.Repository<MechanicSpecialty>().ExistsAsync(
            x => x.SpecialtyId == specialtyId,
            cancellationToken);
        if (!specialtyExists)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.SpecialtyNotFound);
        }

        var mechanicHasSpecialty = await _unitOfWork.Repository<MechanicSpecialtyAssignment>().ExistsAsync(
            x => x.PersonId == mechanicPersonId && x.SpecialtyId == specialtyId,
            cancellationToken);
        if (!mechanicHasSpecialty)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicDoesNotHaveSpecialtyInvalid);
        }

        var duplicateAssignment = await _unitOfWork.Repository<MechanicAssignment>().ExistsAsync(
            x => x.OrderServiceId == orderServiceId && x.MechanicPersonId == mechanicPersonId,
            cancellationToken);
        if (duplicateAssignment)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.DuplicateMechanicAssignmentConflict);
        }

        var mechanicAssignment = new MechanicAssignment
        {
            OrderServiceId = orderServiceId,
            MechanicPersonId = mechanicPersonId,
            SpecialtyId = specialtyId
        };

        await _unitOfWork.Repository<MechanicAssignment>().AddAsync(mechanicAssignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceExecutionResultDto>.Success(SuccessResult(mechanicAssignment.MechanicAssignmentId, "MechanicAssignment", "AssignMechanic"));
    }

    public async Task<Result<ServiceExecutionResultDto>> UnassignMechanicAsync(
        int orderServiceId,
        UnassignMechanicRequest request,
        CancellationToken cancellationToken = default)
    {
        if (orderServiceId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceIdInvalid);
        }

        var mechanicAssignmentId = request?.MechanicAssignmentId ?? 0;
        if (mechanicAssignmentId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicAssignmentIdInvalid);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        var mechanicAssignment = await mechanicAssignmentRepository.GetByIdAsync(mechanicAssignmentId, cancellationToken);
        if (mechanicAssignment is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicAssignmentNotFound);
        }

        if (mechanicAssignment.OrderServiceId != orderServiceId)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicAssignmentDoesNotBelongToOrderServiceConflict);
        }

        mechanicAssignmentRepository.Remove(mechanicAssignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceExecutionResultDto>.Success(SuccessResult(mechanicAssignmentId, "MechanicAssignment", "UnassignMechanic"));
    }

    public async Task<Result<ServiceExecutionResultDto>> RequestPartAsync(
        int orderServiceId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        RequestOrderServicePartRequest request,
        CancellationToken cancellationToken = default)
    {
        if (orderServiceId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceIdInvalid);
        }

        if (currentPersonId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var partId = request?.PartId ?? 0;
        var quantity = request?.Quantity ?? 0;
        var appliedUnitPrice = request?.AppliedUnitPrice;

        if (partId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PartIdInvalid);
        }

        if (quantity <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.QuantityInvalid);
        }

        if (appliedUnitPrice.HasValue && appliedUnitPrice.Value < 0m)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.AppliedUnitPriceInvalid);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        if (!HasRole(currentRoles, AdminRoleName))
        {
            var assigned = await IsMechanicAssignedToOrderServiceAsync(orderServiceId, currentPersonId, cancellationToken);
            if (!assigned)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicNotAssignedConflict);
            }
        }

        var part = await _unitOfWork.Repository<Part>().GetByIdAsync(partId, cancellationToken);
        if (part is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PartNotFound);
        }

        if (!part.IsActive)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PartInactive);
        }

        var duplicatePart = await _unitOfWork.Repository<OrderServicePart>().ExistsAsync(
            x => x.OrderServiceId == orderServiceId && x.PartId == partId,
            cancellationToken);
        if (duplicatePart)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.DuplicatePartForOrderServiceConflict);
        }

        var resolvedUnitPrice = appliedUnitPrice ?? part.UnitPrice;
        if (resolvedUnitPrice < 0m)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.AppliedUnitPriceInvalid);
        }

        var orderServicePart = new OrderServicePart
        {
            OrderServiceId = orderServiceId,
            PartId = partId,
            Quantity = quantity,
            AppliedUnitPrice = resolvedUnitPrice,
            CustomerApproved = null,
            ApprovalDate = null
        };

        await _unitOfWork.Repository<OrderServicePart>().AddAsync(orderServicePart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServicePart.OrderServicePartId, "OrderServicePart", "RequestPart"));
    }

    public async Task<Result<ServiceExecutionResultDto>> ApproveOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServicePartApprovalAsync(
            orderServicePartId,
            currentPersonId,
            currentRoles,
            approve: true,
            cancellationToken);
    }

    public async Task<Result<ServiceExecutionResultDto>> RejectOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServicePartApprovalAsync(
            orderServicePartId,
            currentPersonId,
            currentRoles,
            approve: false,
            cancellationToken);
    }

    public async Task<Result<ServiceExecutionResultDto>> ChangeOrderServicePartQuantityAsync(
        int orderServicePartId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        ChangeOrderServicePartQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (orderServicePartId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServicePartIdInvalid);
        }

        if (currentPersonId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var newQuantity = request?.Quantity ?? 0;
        if (newQuantity <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.QuantityInvalid);
        }

        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServicePart = await orderServicePartRepository.GetByIdAsync(orderServicePartId, cancellationToken);
        if (orderServicePart is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServicePartNotFound);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServicePart.OrderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        if (!HasRole(currentRoles, AdminRoleName))
        {
            var assigned = await IsMechanicAssignedToOrderServiceAsync(orderService.OrderServiceId, currentPersonId, cancellationToken);
            if (!assigned)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.MechanicNotAssignedConflict);
            }
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(orderServicePart.PartId, cancellationToken);
        if (part is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PartNotFound);
        }

        var oldQuantity = orderServicePart.Quantity;
        if (orderServicePart.CustomerApproved == true)
        {
            var difference = newQuantity - oldQuantity;

            if (difference > 0 && part.Stock < difference)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.InsufficientStockConflict);
            }

            var stockAfterAdjustment = part.Stock - difference;
            if (stockAfterAdjustment < 0)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.StockWouldBeNegativeInvalid);
            }

            part.Stock = stockAfterAdjustment;
            partRepository.Update(part);
        }

        orderServicePart.Quantity = newQuantity;
        orderServicePartRepository.Update(orderServicePart);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServicePartId, "OrderServicePart", "ChangeQuantity"));
    }

    public async Task<Result<PendingApprovalDto>> GetClientPendingApprovalsAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        var clientValidation = await ValidateClientPersonAsync(currentPersonId, cancellationToken);
        if (clientValidation is not null)
        {
            return Result<PendingApprovalDto>.Failure(clientValidation);
        }

        var currentVehicleIds = await _unitOfWork.Repository<VehicleOwnerHistory>().FindAsync(
            x => x.PersonId == currentPersonId && x.EndDate == null,
            cancellationToken);

        var vehicleIds = currentVehicleIds
            .Select(x => x.VehicleId)
            .Distinct()
            .ToList();

        if (vehicleIds.Count == 0)
        {
            return Result<PendingApprovalDto>.Success(new PendingApprovalDto());
        }

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var serviceOrderById = serviceOrders.ToDictionary(x => x.ServiceOrderId, x => x);
        var serviceOrderIds = serviceOrderById.Keys.ToList();

        if (serviceOrderIds.Count == 0)
        {
            return Result<PendingApprovalDto>.Success(new PendingApprovalDto());
        }

        var allOrderServices = await _unitOfWork.Repository<OrderService>().FindAsync(
            x => serviceOrderIds.Contains(x.ServiceOrderId),
            cancellationToken);

        var allOrderServiceById = allOrderServices.ToDictionary(x => x.OrderServiceId, x => x);
        var allOrderServiceIds = allOrderServiceById.Keys.ToList();

        var pendingOrderServices = allOrderServices
            .Where(x => x.CustomerApproved == null)
            .ToList();

        var pendingOrderServiceParts = allOrderServiceIds.Count == 0
            ? Array.Empty<OrderServicePart>()
            : (await _unitOfWork.Repository<OrderServicePart>().FindAsync(
                x => allOrderServiceIds.Contains(x.OrderServiceId) && x.CustomerApproved == null,
                cancellationToken)).ToArray();

        var orderServiceDtos = pendingOrderServices
            .OrderBy(x => x.ServiceOrderId)
            .ThenBy(x => x.OrderServiceId)
            .Where(x => serviceOrderById.ContainsKey(x.ServiceOrderId))
            .Select(x =>
            {
                var serviceOrder = serviceOrderById[x.ServiceOrderId];
                return new PendingOrderServiceApprovalDto
                {
                    OrderServiceId = x.OrderServiceId,
                    ServiceOrderId = x.ServiceOrderId,
                    VehicleId = serviceOrder.VehicleId,
                    ServiceTypeId = x.ServiceTypeId,
                    Description = x.Description,
                    WorkPerformed = x.WorkPerformed,
                    LaborCost = x.LaborCost
                };
            })
            .ToList();

        var partDtos = pendingOrderServiceParts
            .Where(x => allOrderServiceById.ContainsKey(x.OrderServiceId))
            .OrderBy(x => allOrderServiceById[x.OrderServiceId].ServiceOrderId)
            .ThenBy(x => x.OrderServiceId)
            .ThenBy(x => x.OrderServicePartId)
            .Select(x =>
            {
                var orderService = allOrderServiceById[x.OrderServiceId];
                var serviceOrder = serviceOrderById[orderService.ServiceOrderId];

                return new PendingOrderServicePartApprovalDto
                {
                    OrderServicePartId = x.OrderServicePartId,
                    OrderServiceId = x.OrderServiceId,
                    ServiceOrderId = orderService.ServiceOrderId,
                    VehicleId = serviceOrder.VehicleId,
                    PartId = x.PartId,
                    Quantity = x.Quantity,
                    AppliedUnitPrice = x.AppliedUnitPrice,
                    Subtotal = x.Quantity * x.AppliedUnitPrice
                };
            })
            .ToList();

        return Result<PendingApprovalDto>.Success(new PendingApprovalDto
        {
            OrderServices = orderServiceDtos,
            OrderServiceParts = partDtos
        });
    }

    public async Task<Result<ServiceExecutionResultDto>> ApproveOrderServiceAsync(
        int orderServiceId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServiceApprovalAsync(orderServiceId, currentPersonId, approve: true, cancellationToken);
    }

    public async Task<Result<ServiceExecutionResultDto>> RejectOrderServiceAsync(
        int orderServiceId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await SetOrderServiceApprovalAsync(orderServiceId, currentPersonId, approve: false, cancellationToken);
    }

    public async Task<Result<ServiceExecutionResultDto>> ClientApproveOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await ApproveOrderServicePartAsync(
            orderServicePartId,
            currentPersonId,
            new[] { ClientRoleName },
            cancellationToken);
    }

    public async Task<Result<ServiceExecutionResultDto>> ClientRejectOrderServicePartAsync(
        int orderServicePartId,
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        return await RejectOrderServicePartAsync(
            orderServicePartId,
            currentPersonId,
            new[] { ClientRoleName },
            cancellationToken);
    }

    private async Task<Result<ServiceExecutionResultDto>> SetOrderServiceApprovalAsync(
        int orderServiceId,
        int currentPersonId,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (orderServiceId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceIdInvalid);
        }

        if (currentPersonId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var clientValidation = await ValidateClientPersonAsync(currentPersonId, cancellationToken);
        if (clientValidation is not null)
        {
            return Result<ServiceExecutionResultDto>.Failure(clientValidation);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderService = await orderServiceRepository.GetByIdAsync(orderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        var canAccess = await IsCurrentOwnerOfVehicleAsync(serviceOrder.VehicleId, currentPersonId, cancellationToken);
        if (!canAccess)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ClientCannotAccessServiceOrderConflict);
        }

        if (orderService.CustomerApproved == approve)
        {
            return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServiceId, "OrderService", approve ? "Approve" : "Reject"));
        }

        orderService.CustomerApproved = approve;
        orderService.ApprovalDate = DateTime.UtcNow;

        orderServiceRepository.Update(orderService);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServiceId, "OrderService", approve ? "Approve" : "Reject"));
    }

    private async Task<Result<ServiceExecutionResultDto>> SetOrderServicePartApprovalAsync(
        int orderServicePartId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (orderServicePartId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServicePartIdInvalid);
        }

        if (currentPersonId <= 0)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.CurrentPersonIdInvalid);
        }

        var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
        var orderServicePart = await orderServicePartRepository.GetByIdAsync(orderServicePartId, cancellationToken);
        if (orderServicePart is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServicePartNotFound);
        }

        var orderService = await _unitOfWork.Repository<OrderService>().GetByIdAsync(orderServicePart.OrderServiceId, cancellationToken);
        if (orderService is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.OrderServiceNotFound);
        }

        var serviceOrder = await _unitOfWork.Repository<ServiceOrder>().GetByIdAsync(orderService.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderNotFound);
        }

        if (await IsServiceOrderBlockedAsync(serviceOrder, cancellationToken))
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ServiceOrderCannotBeModifiedConflict);
        }

        if (!HasRole(currentRoles, AdminRoleName))
        {
            var canAccess = await IsCurrentOwnerOfVehicleAsync(serviceOrder.VehicleId, currentPersonId, cancellationToken);
            if (!canAccess)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.ClientCannotAccessServiceOrderConflict);
            }
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var part = await partRepository.GetByIdAsync(orderServicePart.PartId, cancellationToken);
        if (part is null)
        {
            return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.PartNotFound);
        }

        if (approve)
        {
            if (orderServicePart.CustomerApproved == true)
            {
                return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServicePartId, "OrderServicePart", "Approve"));
            }

            if (part.Stock < orderServicePart.Quantity)
            {
                return Result<ServiceExecutionResultDto>.Failure(ServiceExecutionErrors.InsufficientStockConflict);
            }

            part.Stock -= orderServicePart.Quantity;
            partRepository.Update(part);

            orderServicePart.CustomerApproved = true;
            orderServicePart.ApprovalDate = DateTime.UtcNow;
            orderServicePartRepository.Update(orderServicePart);
        }
        else
        {
            if (orderServicePart.CustomerApproved == true)
            {
                part.Stock += orderServicePart.Quantity;
                partRepository.Update(part);
            }

            orderServicePart.CustomerApproved = false;
            orderServicePart.ApprovalDate = DateTime.UtcNow;
            orderServicePartRepository.Update(orderServicePart);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ServiceExecutionResultDto>.Success(SuccessResult(orderServicePartId, "OrderServicePart", approve ? "Approve" : "Reject"));
    }

    private async Task<bool> IsServiceOrderBlockedAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken)
    {
        var orderStatus = await _unitOfWork.Repository<OrderStatus>().GetByIdAsync(serviceOrder.OrderStatusId, cancellationToken);
        if (orderStatus is null)
        {
            return true;
        }

        return orderStatus.Name.Equals(CancelledStatusName, StringComparison.OrdinalIgnoreCase) ||
               orderStatus.Name.Equals(VoidedStatusName, StringComparison.OrdinalIgnoreCase) ||
               orderStatus.Name.Equals(CompletedStatusName, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> IsMechanicAssignedToOrderServiceAsync(int orderServiceId, int mechanicPersonId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<MechanicAssignment>().ExistsAsync(
            x => x.OrderServiceId == orderServiceId && x.MechanicPersonId == mechanicPersonId,
            cancellationToken);
    }

    private async Task<bool> IsCurrentOwnerOfVehicleAsync(int vehicleId, int personId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<VehicleOwnerHistory>().ExistsAsync(
            x => x.VehicleId == vehicleId && x.PersonId == personId && x.EndDate == null,
            cancellationToken);
    }

    private async Task<Error?> ValidateClientPersonAsync(int personId, CancellationToken cancellationToken)
    {
        if (personId <= 0)
        {
            return ServiceExecutionErrors.CurrentPersonIdInvalid;
        }

        var personExists = await _unitOfWork.Repository<Person>().ExistsAsync(
            x => x.PersonId == personId,
            cancellationToken);

        if (!personExists)
        {
            return ServiceExecutionErrors.ClientCannotAccessServiceOrderConflict;
        }

        var clientRoleId = await GetRoleIdByNameAsync(ClientRoleName, cancellationToken);
        if (!clientRoleId.HasValue)
        {
            return ServiceExecutionErrors.ClientCannotAccessServiceOrderConflict;
        }

        var hasActiveClientRole = await _unitOfWork.Repository<PersonRole>().ExistsAsync(
            x => x.PersonId == personId && x.RoleId == clientRoleId.Value && x.IsActive,
            cancellationToken);

        return hasActiveClientRole
            ? null
            : ServiceExecutionErrors.ClientCannotAccessServiceOrderConflict;
    }

    private async Task<int?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);

        return roles
            .Where(x => x.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
    }

    private async Task<int[]> GetOrderStatusIdsByNamesAsync(
        IReadOnlyCollection<string> orderStatusNames,
        CancellationToken cancellationToken)
    {
        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var orderStatusNameSet = new HashSet<string>(orderStatusNames, StringComparer.OrdinalIgnoreCase);

        return orderStatuses
            .Where(x => orderStatusNameSet.Contains(x.Name))
            .Select(x => x.OrderStatusId)
            .Distinct()
            .ToArray();
    }

    private static bool HasRole(IReadOnlyList<string> currentRoles, string roleName)
    {
        return currentRoles.Any(x => x.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ServiceExecutionResultDto SuccessResult(int id, string entity, string action)
    {
        return new ServiceExecutionResultDto
        {
            Id = id,
            Entity = entity,
            Action = action,
            Success = true
        };
    }
}
