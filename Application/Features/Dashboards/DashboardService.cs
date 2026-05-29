using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Dashboards.Dtos;
using Application.Features.Dashboards.Errors;
using Domain.Entities;

namespace Application.Features.Dashboards;

public class DashboardService : IDashboardService
{
    private const string ClientRoleName = "Client";
    private const string MechanicRoleName = "Mechanic";
    private const string PendingStatusName = "Pending";
    private const string InProgressStatusName = "InProgress";
    private const string CompletedStatusName = "Completed";
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";
    private const string CompletedPaymentStatusName = "Completed";

    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClientDashboardDto>> GetClientDashboardAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidatePersonWithActiveRoleAsync(currentPersonId, ClientRoleName, cancellationToken);
        if (validationError is not null)
        {
            return Result<ClientDashboardDto>.Failure(validationError);
        }

        var ownerHistories = await _unitOfWork.Repository<VehicleOwnerHistory>().FindAsync(
            x => x.PersonId == currentPersonId && x.EndDate == null,
            cancellationToken);

        var vehicleIds = ownerHistories
            .Select(x => x.VehicleId)
            .Distinct()
            .ToList();

        if (vehicleIds.Count == 0)
        {
            return Result<ClientDashboardDto>.Success(new ClientDashboardDto
            {
                TotalVehicles = 0
            });
        }

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().FindAsync(
            x => vehicleIds.Contains(x.VehicleId),
            cancellationToken);

        var blockedStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName, CompletedStatusName },
            cancellationToken);

        var activeServiceOrders = serviceOrders.Count(x => !blockedStatusIds.Contains(x.OrderStatusId));
        var recentServiceOrderIds = serviceOrders
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ServiceOrderId)
            .Take(5)
            .Select(x => x.ServiceOrderId)
            .ToList();

        var serviceOrderIds = serviceOrders
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToList();

        var allOrderServices = serviceOrderIds.Count == 0
            ? Array.Empty<OrderService>()
            : (await _unitOfWork.Repository<OrderService>().FindAsync(
                x => serviceOrderIds.Contains(x.ServiceOrderId),
                cancellationToken)).ToArray();

        var pendingOrderServices = allOrderServices.Count(x => x.CustomerApproved is null);
        var orderServiceIds = allOrderServices
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToList();

        var pendingOrderServiceParts = orderServiceIds.Count == 0
            ? 0
            : await _unitOfWork.Repository<OrderServicePart>().CountAsync(
                x => orderServiceIds.Contains(x.OrderServiceId) && x.CustomerApproved == null,
                cancellationToken);

        var pendingApprovals = pendingOrderServices + pendingOrderServiceParts;

        var invoices = serviceOrderIds.Count == 0
            ? Array.Empty<Invoice>()
            : (await _unitOfWork.Repository<Invoice>().FindAsync(
                x => serviceOrderIds.Contains(x.ServiceOrderId),
                cancellationToken)).ToArray();

        var pendingInvoices = await CountPendingInvoicesAsync(invoices, cancellationToken);

        return Result<ClientDashboardDto>.Success(new ClientDashboardDto
        {
            ActiveServiceOrders = activeServiceOrders,
            PendingApprovals = pendingApprovals,
            PendingInvoices = pendingInvoices,
            TotalVehicles = vehicleIds.Count,
            RecentServiceOrderIds = recentServiceOrderIds
        });
    }

    public async Task<Result<MechanicDashboardDto>> GetMechanicDashboardAsync(
        int currentPersonId,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidatePersonWithActiveRoleAsync(currentPersonId, MechanicRoleName, cancellationToken);
        if (validationError is not null)
        {
            return Result<MechanicDashboardDto>.Failure(validationError);
        }

        var assignments = await _unitOfWork.Repository<MechanicAssignment>().FindAsync(
            x => x.MechanicPersonId == currentPersonId,
            cancellationToken);

        if (assignments.Count == 0)
        {
            return Result<MechanicDashboardDto>.Success(new MechanicDashboardDto());
        }

        var assignedServices = assignments.Count;
        var orderServiceIds = assignments
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToList();

        var orderServices = await _unitOfWork.Repository<OrderService>().FindAsync(
            x => orderServiceIds.Contains(x.OrderServiceId),
            cancellationToken);

        var pendingWorkReports = orderServices.Count(x => string.IsNullOrWhiteSpace(x.WorkPerformed));

        var serviceOrderIds = orderServices
            .Select(x => x.ServiceOrderId)
            .Distinct()
            .ToList();

        var serviceOrders = serviceOrderIds.Count == 0
            ? Array.Empty<ServiceOrder>()
            : (await _unitOfWork.Repository<ServiceOrder>().FindAsync(
                x => serviceOrderIds.Contains(x.ServiceOrderId),
                cancellationToken)).ToArray();

        var blockedStatusIds = await GetOrderStatusIdsByNamesAsync(
            new[] { CancelledStatusName, VoidedStatusName, CompletedStatusName },
            cancellationToken);

        var activeServiceOrderIds = serviceOrders
            .Where(x => !blockedStatusIds.Contains(x.OrderStatusId))
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.ServiceOrderId)
            .Select(x => x.ServiceOrderId)
            .ToList();

        var requestedPartsPendingApproval = orderServiceIds.Count == 0
            ? 0
            : await _unitOfWork.Repository<OrderServicePart>().CountAsync(
                x => orderServiceIds.Contains(x.OrderServiceId) && x.CustomerApproved == null,
                cancellationToken);

        return Result<MechanicDashboardDto>.Success(new MechanicDashboardDto
        {
            AssignedServices = assignedServices,
            ActiveOrders = activeServiceOrderIds.Count,
            PendingWorkReports = pendingWorkReports,
            RequestedPartsPendingApproval = requestedPartsPendingApproval,
            ActiveServiceOrderIds = activeServiceOrderIds
        });
    }

    public async Task<Result<ReceptionistDashboardDto>> GetReceptionistDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var pendingStatusId = GetStatusId(orderStatuses, PendingStatusName);
        var inProgressStatusId = GetStatusId(orderStatuses, InProgressStatusName);
        var completedStatusId = GetStatusId(orderStatuses, CompletedStatusName);
        var blockedStatusIds = GetStatusIds(orderStatuses, new[] { CancelledStatusName, VoidedStatusName, CompletedStatusName });

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;

        var pendingOrders = pendingStatusId.HasValue
            ? serviceOrders.Count(x => x.OrderStatusId == pendingStatusId.Value)
            : 0;

        var inProgressOrders = inProgressStatusId.HasValue
            ? serviceOrders.Count(x => x.OrderStatusId == inProgressStatusId.Value)
            : 0;

        var completedOrdersToday = completedStatusId.HasValue
            ? serviceOrders.Count(x => x.OrderStatusId == completedStatusId.Value && x.CreatedAt.Date == today)
            : 0;

        var vehiclesCurrentlyInWorkshop = serviceOrders
            .Where(x => !blockedStatusIds.Contains(x.OrderStatusId))
            .Select(x => x.VehicleId)
            .Distinct()
            .Count();

        var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);
        var pendingInvoices = await CountPendingInvoicesAsync(invoices, cancellationToken);

        var lowStockParts = await _unitOfWork.Repository<Part>().CountAsync(
            x => x.IsActive && x.Stock <= x.MinimumStock,
            cancellationToken);

        return Result<ReceptionistDashboardDto>.Success(new ReceptionistDashboardDto
        {
            PendingOrders = pendingOrders,
            InProgressOrders = inProgressOrders,
            CompletedOrdersToday = completedOrdersToday,
            VehiclesCurrentlyInWorkshop = vehiclesCurrentlyInWorkshop,
            PendingInvoices = pendingInvoices,
            LowStockParts = lowStockParts
        });
    }

    public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);

        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);
        var clientRoleId = roles
            .Where(x => x.RoleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
        var mechanicRoleId = roles
            .Where(x => x.RoleName.Equals(MechanicRoleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();

        var personRoles = await _unitOfWork.Repository<PersonRole>().GetAllAsync(cancellationToken);
        var totalClients = clientRoleId.HasValue
            ? personRoles.Count(x => x.RoleId == clientRoleId.Value && x.IsActive)
            : 0;
        var totalMechanics = mechanicRoleId.HasValue
            ? personRoles.Count(x => x.RoleId == mechanicRoleId.Value && x.IsActive)
            : 0;

        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var pendingStatusId = GetStatusId(orderStatuses, PendingStatusName);
        var inProgressStatusId = GetStatusId(orderStatuses, InProgressStatusName);
        var completedStatusId = GetStatusId(orderStatuses, CompletedStatusName);
        var blockedStatusIds = GetStatusIds(orderStatuses, new[] { CancelledStatusName, VoidedStatusName, CompletedStatusName });

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().GetAllAsync(cancellationToken);
        var activeServiceOrders = serviceOrders.Count(x => !blockedStatusIds.Contains(x.OrderStatusId));
        var pendingOrders = pendingStatusId.HasValue ? serviceOrders.Count(x => x.OrderStatusId == pendingStatusId.Value) : 0;
        var inProgressOrders = inProgressStatusId.HasValue ? serviceOrders.Count(x => x.OrderStatusId == inProgressStatusId.Value) : 0;
        var completedOrders = completedStatusId.HasValue ? serviceOrders.Count(x => x.OrderStatusId == completedStatusId.Value) : 0;

        var lowStockParts = await _unitOfWork.Repository<Part>().CountAsync(
            x => x.IsActive && x.Stock <= x.MinimumStock,
            cancellationToken);

        var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);
        var totalInvoicedAmount = invoices.Sum(x => x.Total);

        var completedPaymentStatusIds = await GetPaymentStatusIdsByNameAsync(CompletedPaymentStatusName, cancellationToken);
        var payments = completedPaymentStatusIds.Length == 0
            ? Array.Empty<Payment>()
            : (await _unitOfWork.Repository<Payment>().FindAsync(
                x => completedPaymentStatusIds.Contains(x.PaymentStatusId),
                cancellationToken)).ToArray();

        var totalCompletedPaymentsAmount = payments.Sum(x => x.Amount);
        var pendingPaymentAmount = totalInvoicedAmount - totalCompletedPaymentsAmount;
        if (pendingPaymentAmount < 0m)
        {
            pendingPaymentAmount = 0m;
        }

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(x => x.IsActive),
            TotalClients = totalClients,
            TotalMechanics = totalMechanics,
            ActiveServiceOrders = activeServiceOrders,
            PendingOrders = pendingOrders,
            InProgressOrders = inProgressOrders,
            CompletedOrders = completedOrders,
            LowStockParts = lowStockParts,
            TotalInvoicedAmount = totalInvoicedAmount,
            TotalCompletedPaymentsAmount = totalCompletedPaymentsAmount,
            PendingPaymentAmount = pendingPaymentAmount
        });
    }

    private async Task<Error?> ValidatePersonWithActiveRoleAsync(
        int currentPersonId,
        string roleName,
        CancellationToken cancellationToken)
    {
        if (currentPersonId <= 0)
        {
            return DashboardErrors.CurrentPersonIdInvalid;
        }

        var personExists = await _unitOfWork.Repository<Person>().ExistsAsync(
            x => x.PersonId == currentPersonId,
            cancellationToken);
        if (!personExists)
        {
            return DashboardErrors.PersonNotFound;
        }

        var roleId = await GetRoleIdByNameAsync(roleName, cancellationToken);
        if (!roleId.HasValue)
        {
            return roleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase)
                ? DashboardErrors.PersonIsNotClientInvalid
                : DashboardErrors.PersonIsNotMechanicInvalid;
        }

        var hasRole = await _unitOfWork.Repository<PersonRole>().ExistsAsync(
            x => x.PersonId == currentPersonId && x.RoleId == roleId.Value && x.IsActive,
            cancellationToken);

        if (hasRole)
        {
            return null;
        }

        return roleName.Equals(ClientRoleName, StringComparison.OrdinalIgnoreCase)
            ? DashboardErrors.PersonIsNotClientInvalid
            : DashboardErrors.PersonIsNotMechanicInvalid;
    }

    private async Task<int> CountPendingInvoicesAsync(IReadOnlyList<Invoice> invoices, CancellationToken cancellationToken)
    {
        if (invoices.Count == 0)
        {
            return 0;
        }

        var completedStatusIds = await GetPaymentStatusIdsByNameAsync(CompletedPaymentStatusName, cancellationToken);
        if (completedStatusIds.Length == 0)
        {
            return invoices.Count(x => x.Total > 0m);
        }

        var invoiceIds = invoices
            .Select(x => x.InvoiceId)
            .Distinct()
            .ToList();

        var completedPayments = await _unitOfWork.Repository<Payment>().FindAsync(
            x => invoiceIds.Contains(x.InvoiceId) && completedStatusIds.Contains(x.PaymentStatusId),
            cancellationToken);

        var completedAmountByInvoice = completedPayments
            .GroupBy(x => x.InvoiceId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Amount));

        return invoices.Count(invoice =>
        {
            var completedAmount = completedAmountByInvoice.TryGetValue(invoice.InvoiceId, out var paid)
                ? paid
                : 0m;

            return invoice.Total > completedAmount;
        });
    }

    private async Task<int[]> GetOrderStatusIdsByNamesAsync(
        IReadOnlyCollection<string> statusNames,
        CancellationToken cancellationToken)
    {
        var orderStatuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        return GetStatusIds(orderStatuses, statusNames);
    }

    private async Task<int[]> GetPaymentStatusIdsByNameAsync(string statusName, CancellationToken cancellationToken)
    {
        var paymentStatuses = await _unitOfWork.Repository<PaymentStatus>().GetAllAsync(cancellationToken);

        return paymentStatuses
            .Where(x => x.Name.Equals(statusName, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.PaymentStatusId)
            .Distinct()
            .ToArray();
    }

    private async Task<int?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);

        return roles
            .Where(x => x.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();
    }

    private static int? GetStatusId(IReadOnlyList<OrderStatus> statuses, string statusName)
    {
        return statuses
            .Where(x => x.Name.Equals(statusName, StringComparison.OrdinalIgnoreCase))
            .Select(x => (int?)x.OrderStatusId)
            .FirstOrDefault();
    }

    private static int[] GetStatusIds(IReadOnlyList<OrderStatus> statuses, IReadOnlyCollection<string> statusNames)
    {
        var statusNameSet = new HashSet<string>(statusNames, StringComparer.OrdinalIgnoreCase);

        return statuses
            .Where(x => statusNameSet.Contains(x.Name))
            .Select(x => x.OrderStatusId)
            .Distinct()
            .ToArray();
    }
}
