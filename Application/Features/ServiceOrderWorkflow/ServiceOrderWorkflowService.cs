using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.ServiceOrderWorkflow.Dtos;
using Application.Features.ServiceOrderWorkflow.Errors;
using Application.Features.ServiceOrderWorkflow.Requests;
using Domain.Entities;

namespace Application.Features.ServiceOrderWorkflow;

public class ServiceOrderWorkflowService : IServiceOrderWorkflowService
{
    private const string AdminRoleName = "Admin";
    private const string ReceptionistRoleName = "Receptionist";
    private const string MechanicRoleName = "Mechanic";
    private const string ClientRoleName = "Client";

    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";
    private const string CompletedStatusName = "Completed";

    private const string CompletedObservation = "Service order completed.";

    private readonly IUnitOfWork _unitOfWork;

    public ServiceOrderWorkflowService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceOrderFullDetailDto>> GetFullDetailAsync(
        int serviceOrderId,
        int currentUserId,
        int currentPersonId,
        IReadOnlyList<string> currentRoles,
        CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return Result<ServiceOrderFullDetailDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderIdInvalid);
        }

        if (currentUserId <= 0)
        {
            return Result<ServiceOrderFullDetailDto>.Failure(ServiceOrderWorkflowErrors.ChangedByUserIdInvalid);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderFullDetailDto>.Failure(ServiceOrderWorkflowErrors.NotFound);
        }

        var roleSet = new HashSet<string>(currentRoles ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(ClientRoleName))
        {
            var canAccessAsClient = await CanClientAccessServiceOrderAsync(serviceOrder.VehicleId, currentPersonId, cancellationToken);
            if (!canAccessAsClient)
            {
                return Result<ServiceOrderFullDetailDto>.Failure(ServiceOrderWorkflowErrors.ClientCannotAccessServiceOrderConflict);
            }
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderServices = await orderServiceRepository.FindAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (roleSet.Contains(MechanicRoleName) &&
            !roleSet.Contains(AdminRoleName) &&
            !roleSet.Contains(ReceptionistRoleName) &&
            !roleSet.Contains(ClientRoleName))
        {
            var canAccessAsMechanic = await CanMechanicAccessServiceOrderAsync(
                currentPersonId,
                orderServices.Select(x => x.OrderServiceId).ToList(),
                cancellationToken);

            if (!canAccessAsMechanic)
            {
                return Result<ServiceOrderFullDetailDto>.Failure(ServiceOrderWorkflowErrors.MechanicCannotAccessServiceOrderConflict);
            }
        }

        var inventoryRepository = _unitOfWork.Repository<VehicleEntryInventory>();
        var inventory = (await inventoryRepository.FindAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken)).FirstOrDefault();

        var orderServiceIds = orderServices.Select(x => x.OrderServiceId).ToList();

        var mechanicAssignments = orderServiceIds.Count == 0
            ? Array.Empty<MechanicAssignment>()
            : (await _unitOfWork.Repository<MechanicAssignment>().FindAsync(
                x => orderServiceIds.Contains(x.OrderServiceId),
                cancellationToken)).ToArray();

        var orderServiceParts = orderServiceIds.Count == 0
            ? Array.Empty<OrderServicePart>()
            : (await _unitOfWork.Repository<OrderServicePart>().FindAsync(
                x => orderServiceIds.Contains(x.OrderServiceId),
                cancellationToken)).ToArray();

        var mechanicsByOrderServiceId = mechanicAssignments
            .GroupBy(x => x.OrderServiceId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<ServiceOrderMechanicSummaryDto>)x
                    .OrderBy(y => y.MechanicAssignmentId)
                    .Select(MapMechanicSummary)
                    .ToList());

        var partsByOrderServiceId = orderServiceParts
            .GroupBy(x => x.OrderServiceId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<ServiceOrderPartSummaryDto>)x
                    .OrderBy(y => y.OrderServicePartId)
                    .Select(MapPartSummary)
                    .ToList());

        var serviceSummaries = orderServices
            .OrderBy(x => x.OrderServiceId)
            .Select(x => new ServiceOrderServiceSummaryDto
            {
                OrderServiceId = x.OrderServiceId,
                ServiceTypeId = x.ServiceTypeId,
                Description = x.Description,
                WorkPerformed = x.WorkPerformed,
                LaborCost = x.LaborCost,
                CustomerApproved = x.CustomerApproved,
                ApprovalDate = x.ApprovalDate,
                Mechanics = mechanicsByOrderServiceId.TryGetValue(x.OrderServiceId, out var mechanics)
                    ? mechanics
                    : Array.Empty<ServiceOrderMechanicSummaryDto>(),
                Parts = partsByOrderServiceId.TryGetValue(x.OrderServiceId, out var parts)
                    ? parts
                    : Array.Empty<ServiceOrderPartSummaryDto>()
            })
            .ToList();

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = (await invoiceRepository.FindAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken)).FirstOrDefault();

        ServiceOrderInvoiceSummaryDto? invoiceSummary = null;
        if (invoice is not null)
        {
            var paymentRepository = _unitOfWork.Repository<Payment>();
            var payments = await paymentRepository.FindAsync(
                x => x.InvoiceId == invoice.InvoiceId,
                cancellationToken);

            invoiceSummary = new ServiceOrderInvoiceSummaryDto
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceStatusId = invoice.InvoiceStatusId,
                InvoiceDate = invoice.InvoiceDate,
                Subtotal = invoice.Subtotal,
                Tax = invoice.Tax,
                Total = invoice.Total,
                Payments = payments
                    .OrderBy(x => x.PaymentId)
                    .Select(x => new ServiceOrderPaymentSummaryDto
                    {
                        PaymentId = x.PaymentId,
                        PaymentMethodId = x.PaymentMethodId,
                        PaymentStatusId = x.PaymentStatusId,
                        PaymentDate = x.PaymentDate,
                        Amount = x.Amount,
                        Reference = x.Reference
                    })
                    .ToList()
            };
        }

        var fullDetail = new ServiceOrderFullDetailDto
        {
            ServiceOrderId = serviceOrder.ServiceOrderId,
            VehicleId = serviceOrder.VehicleId,
            OrderStatusId = serviceOrder.OrderStatusId,
            EntryDate = serviceOrder.EntryDate,
            EstimatedDeliveryDate = serviceOrder.EstimatedDeliveryDate,
            GeneralDescription = serviceOrder.GeneralDescription,
            CancellationReason = serviceOrder.CancellationReason,
            CancellationDate = serviceOrder.CancellationDate,
            CreatedAt = serviceOrder.CreatedAt,
            Inventory = inventory is null ? null : new ServiceOrderInventorySummaryDto
            {
                EntryInventoryId = inventory.EntryInventoryId,
                HasScratches = inventory.HasScratches,
                ScratchesDescription = inventory.ScratchesDescription,
                HasToolbox = inventory.HasToolbox,
                ToolboxDescription = inventory.ToolboxDescription,
                OwnershipCardDelivered = inventory.OwnershipCardDelivered,
                Observations = inventory.Observations,
                RegisteredAt = inventory.RegisteredAt
            },
            Services = serviceSummaries,
            Invoice = invoiceSummary
        };

        return Result<ServiceOrderFullDetailDto>.Success(fullDetail);
    }

    public async Task<Result<ServiceOrderWorkflowDto>> ChangeStatusAsync(
        int serviceOrderId,
        int changedByUserId,
        ChangeServiceOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderIdInvalid);
        }

        var changedByUserValidationError = await ValidateChangedByUserAsync(changedByUserId, cancellationToken);
        if (changedByUserValidationError is not null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(changedByUserValidationError);
        }

        var newOrderStatusId = request?.NewOrderStatusId ?? 0;
        if (newOrderStatusId <= 0)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.NewOrderStatusIdInvalid);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.NotFound);
        }

        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var previousStatus = orderStatuses.FirstOrDefault(x => x.OrderStatusId == serviceOrder.OrderStatusId);
        if (previousStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.PreviousOrderStatusNotFound);
        }

        var newStatus = orderStatuses.FirstOrDefault(x => x.OrderStatusId == newOrderStatusId);
        if (newStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.NewOrderStatusNotFound);
        }

        var history = new OrderStatusHistory
        {
            ServiceOrderId = serviceOrderId,
            PreviousOrderStatusId = previousStatus.OrderStatusId,
            NewOrderStatusId = newStatus.OrderStatusId,
            ChangedByUserId = changedByUserId,
            Observation = NormalizeOptionalText(request?.Observation),
            ChangedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<OrderStatusHistory>().AddAsync(history, cancellationToken);

        serviceOrder.OrderStatusId = newStatus.OrderStatusId;
        serviceOrderRepository.Update(serviceOrder);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceOrderWorkflowDto>.Success(MapWorkflowDto(serviceOrder, history));
    }

    public async Task<Result<ServiceOrderWorkflowDto>> CancelAsync(
        int serviceOrderId,
        int changedByUserId,
        CancelOrVoidServiceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderIdInvalid);
        }

        var changedByUserValidationError = await ValidateChangedByUserAsync(changedByUserId, cancellationToken);
        if (changedByUserValidationError is not null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(changedByUserValidationError);
        }

        var reason = NormalizeOptionalText(request?.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.CancelReasonRequired);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.NotFound);
        }

        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var cancelledStatus = GetStatusByName(orderStatuses, CancelledStatusName);
        if (cancelledStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.CancelledStatusNotFound);
        }

        var voidedStatus = GetStatusByName(orderStatuses, VoidedStatusName);
        if (serviceOrder.OrderStatusId == cancelledStatus.OrderStatusId ||
            (voidedStatus is not null && serviceOrder.OrderStatusId == voidedStatus.OrderStatusId))
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderCannotBeCancelledConflict);
        }

        var previousStatus = orderStatuses.FirstOrDefault(x => x.OrderStatusId == serviceOrder.OrderStatusId);
        if (previousStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.PreviousOrderStatusNotFound);
        }

        var observation = NormalizeOptionalText(request?.Observation) ?? reason;

        var history = new OrderStatusHistory
        {
            ServiceOrderId = serviceOrderId,
            PreviousOrderStatusId = previousStatus.OrderStatusId,
            NewOrderStatusId = cancelledStatus.OrderStatusId,
            ChangedByUserId = changedByUserId,
            Observation = observation,
            ChangedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<OrderStatusHistory>().AddAsync(history, cancellationToken);

        serviceOrder.OrderStatusId = cancelledStatus.OrderStatusId;
        serviceOrder.CancellationReason = reason;
        serviceOrder.CancellationDate = DateTime.UtcNow;

        serviceOrderRepository.Update(serviceOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceOrderWorkflowDto>.Success(MapWorkflowDto(serviceOrder, history));
    }

    public async Task<Result<ServiceOrderWorkflowDto>> VoidAsync(
        int serviceOrderId,
        int changedByUserId,
        CancelOrVoidServiceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderIdInvalid);
        }

        var changedByUserValidationError = await ValidateChangedByUserAsync(changedByUserId, cancellationToken);
        if (changedByUserValidationError is not null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(changedByUserValidationError);
        }

        var reason = NormalizeOptionalText(request?.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.CancelReasonRequired);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.NotFound);
        }

        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var voidedStatus = GetStatusByName(orderStatuses, VoidedStatusName);
        if (voidedStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.VoidedStatusNotFound);
        }

        if (serviceOrder.OrderStatusId == voidedStatus.OrderStatusId)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderCannotBeVoidedConflict);
        }

        var previousStatus = orderStatuses.FirstOrDefault(x => x.OrderStatusId == serviceOrder.OrderStatusId);
        if (previousStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.PreviousOrderStatusNotFound);
        }

        var observation = NormalizeOptionalText(request?.Observation) ?? reason;

        var history = new OrderStatusHistory
        {
            ServiceOrderId = serviceOrderId,
            PreviousOrderStatusId = previousStatus.OrderStatusId,
            NewOrderStatusId = voidedStatus.OrderStatusId,
            ChangedByUserId = changedByUserId,
            Observation = observation,
            ChangedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<OrderStatusHistory>().AddAsync(history, cancellationToken);

        serviceOrder.OrderStatusId = voidedStatus.OrderStatusId;
        serviceOrder.CancellationReason = reason;
        serviceOrder.CancellationDate = DateTime.UtcNow;

        serviceOrderRepository.Update(serviceOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceOrderWorkflowDto>.Success(MapWorkflowDto(serviceOrder, history));
    }

    public async Task<Result<ServiceOrderWorkflowDto>> CompleteAsync(
        int serviceOrderId,
        int changedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderIdInvalid);
        }

        var changedByUserValidationError = await ValidateChangedByUserAsync(changedByUserId, cancellationToken);
        if (changedByUserValidationError is not null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(changedByUserValidationError);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.NotFound);
        }

        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var completedStatus = GetStatusByName(orderStatuses, CompletedStatusName);
        if (completedStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.CompletedStatusNotFound);
        }

        var currentStatus = orderStatuses.FirstOrDefault(x => x.OrderStatusId == serviceOrder.OrderStatusId);
        if (currentStatus is null)
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.PreviousOrderStatusNotFound);
        }

        if (IsStatusName(currentStatus.Name, CancelledStatusName) || IsStatusName(currentStatus.Name, VoidedStatusName))
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderCannotBeCompletedConflict);
        }

        var orderServiceRepository = _unitOfWork.Repository<OrderService>();
        var orderServices = await orderServiceRepository.FindAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (orderServices.Any(x => x.CustomerApproved is null) ||
            orderServices.Any(x => x.CustomerApproved == false) ||
            orderServices.Any(x => string.IsNullOrWhiteSpace(x.WorkPerformed)))
        {
            return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderCannotBeCompletedConflict);
        }

        var orderServiceIds = orderServices.Select(x => x.OrderServiceId).ToList();
        if (orderServiceIds.Count > 0)
        {
            var orderServicePartRepository = _unitOfWork.Repository<OrderServicePart>();
            var orderServiceParts = await orderServicePartRepository.FindAsync(
                x => orderServiceIds.Contains(x.OrderServiceId),
                cancellationToken);

            if (orderServiceParts.Any(x => x.CustomerApproved is null) ||
                orderServiceParts.Any(x => x.CustomerApproved == false))
            {
                return Result<ServiceOrderWorkflowDto>.Failure(ServiceOrderWorkflowErrors.ServiceOrderCannotBeCompletedConflict);
            }
        }

        var history = new OrderStatusHistory
        {
            ServiceOrderId = serviceOrderId,
            PreviousOrderStatusId = currentStatus.OrderStatusId,
            NewOrderStatusId = completedStatus.OrderStatusId,
            ChangedByUserId = changedByUserId,
            Observation = CompletedObservation,
            ChangedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<OrderStatusHistory>().AddAsync(history, cancellationToken);

        serviceOrder.OrderStatusId = completedStatus.OrderStatusId;
        serviceOrderRepository.Update(serviceOrder);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ServiceOrderWorkflowDto>.Success(MapWorkflowDto(serviceOrder, history));
    }

    private async Task<bool> CanClientAccessServiceOrderAsync(int vehicleId, int currentPersonId, CancellationToken cancellationToken)
    {
        if (currentPersonId <= 0)
        {
            return false;
        }

        var vehicleOwnerHistoryRepository = _unitOfWork.Repository<VehicleOwnerHistory>();
        return await vehicleOwnerHistoryRepository.ExistsAsync(
            x => x.VehicleId == vehicleId && x.PersonId == currentPersonId && x.EndDate == null,
            cancellationToken);
    }

    private async Task<bool> CanMechanicAccessServiceOrderAsync(
        int currentPersonId,
        IReadOnlyList<int> orderServiceIds,
        CancellationToken cancellationToken)
    {
        if (currentPersonId <= 0 || orderServiceIds.Count == 0)
        {
            return false;
        }

        var mechanicAssignmentRepository = _unitOfWork.Repository<MechanicAssignment>();
        return await mechanicAssignmentRepository.ExistsAsync(
            x => x.MechanicPersonId == currentPersonId && orderServiceIds.Contains(x.OrderServiceId),
            cancellationToken);
    }

    private async Task<Error?> ValidateChangedByUserAsync(int changedByUserId, CancellationToken cancellationToken)
    {
        if (changedByUserId <= 0)
        {
            return ServiceOrderWorkflowErrors.ChangedByUserIdInvalid;
        }

        var userRepository = _unitOfWork.Repository<User>();
        var userExists = await userRepository.ExistsAsync(
            x => x.UserId == changedByUserId,
            cancellationToken);

        return userExists
            ? null
            : ServiceOrderWorkflowErrors.ChangedByUserNotFound;
    }

    private static ServiceOrderWorkflowDto MapWorkflowDto(ServiceOrder serviceOrder, OrderStatusHistory history)
    {
        return new ServiceOrderWorkflowDto
        {
            ServiceOrderId = serviceOrder.ServiceOrderId,
            PreviousOrderStatusId = history.PreviousOrderStatusId ?? 0,
            NewOrderStatusId = history.NewOrderStatusId,
            OrderStatusHistoryId = history.OrderStatusHistoryId,
            CancellationReason = serviceOrder.CancellationReason,
            CancellationDate = serviceOrder.CancellationDate
        };
    }

    private static ServiceOrderMechanicSummaryDto MapMechanicSummary(MechanicAssignment assignment)
    {
        return new ServiceOrderMechanicSummaryDto
        {
            MechanicAssignmentId = assignment.MechanicAssignmentId,
            MechanicPersonId = assignment.MechanicPersonId,
            SpecialtyId = assignment.SpecialtyId
        };
    }

    private static ServiceOrderPartSummaryDto MapPartSummary(OrderServicePart part)
    {
        return new ServiceOrderPartSummaryDto
        {
            OrderServicePartId = part.OrderServicePartId,
            PartId = part.PartId,
            Quantity = part.Quantity,
            AppliedUnitPrice = part.AppliedUnitPrice,
            Subtotal = part.Quantity * part.AppliedUnitPrice,
            CustomerApproved = part.CustomerApproved,
            ApprovalDate = part.ApprovalDate
        };
    }

    private static OrderStatus? GetStatusByName(IReadOnlyList<OrderStatus> orderStatuses, string statusName)
    {
        return orderStatuses.FirstOrDefault(x => IsStatusName(x.Name, statusName));
    }

    private static bool IsStatusName(string statusName, string expectedStatusName)
    {
        return statusName.Equals(expectedStatusName, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
