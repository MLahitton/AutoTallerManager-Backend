using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Reports.Dtos;
using Application.Features.Reports.Errors;
using Domain.Entities;

namespace Application.Features.Reports;

public class ReportService : IReportService
{
    private const string MechanicRoleName = "Mechanic";
    private const string IssuedInvoiceStatusName = "Issued";
    private const string CancelledInvoiceStatusName = "Cancelled";
    private const string PendingOrderStatusName = "Pending";
    private const string InProgressOrderStatusName = "InProgress";
    private const string CompletedOrderStatusName = "Completed";
    private const string CancelledOrderStatusName = "Cancelled";
    private const string VoidedOrderStatusName = "Voided";
    private const string CompletedPaymentStatusName = "Completed";
    private const string RefundedPaymentStatusName = "Refunded";

    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SalesReportDto>> GetSalesReportAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(from, to))
        {
            return Result<SalesReportDto>.Failure(ReportErrors.DateRangeInvalid);
        }

        var invoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);
        var filteredInvoices = invoices
            .Where(x => IsWithinRange(x.InvoiceDate, from, to))
            .ToList();

        var invoiceStatuses = await _unitOfWork.Repository<InvoiceStatus>().GetAllAsync(cancellationToken);
        var issuedStatusIds = invoiceStatuses
            .Where(x => IsName(x.Name, IssuedInvoiceStatusName))
            .Select(x => x.InvoiceStatusId)
            .ToHashSet();
        var cancelledStatusIds = invoiceStatuses
            .Where(x => IsName(x.Name, CancelledInvoiceStatusName))
            .Select(x => x.InvoiceStatusId)
            .ToHashSet();

        var result = new SalesReportDto
        {
            From = from,
            To = to,
            InvoiceCount = filteredInvoices.Count,
            SubtotalAmount = filteredInvoices.Sum(x => x.Subtotal),
            TaxAmount = filteredInvoices.Sum(x => x.Tax),
            TotalAmount = filteredInvoices.Sum(x => x.Total),
            IssuedInvoices = filteredInvoices.Count(x => issuedStatusIds.Contains(x.InvoiceStatusId)),
            CancelledInvoices = filteredInvoices.Count(x => cancelledStatusIds.Contains(x.InvoiceStatusId))
        };

        return Result<SalesReportDto>.Success(result);
    }

    public async Task<Result<InventoryReportDto>> GetInventoryReportAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(from, to))
        {
            return Result<InventoryReportDto>.Failure(ReportErrors.DateRangeInvalid);
        }

        var parts = await _unitOfWork.Repository<Part>().GetAllAsync(cancellationToken);
        var activeParts = parts.Where(x => x.IsActive).ToList();

        var purchases = await _unitOfWork.Repository<PartPurchase>().GetAllAsync(cancellationToken);
        var filteredPurchases = purchases
            .Where(x => IsWithinRange(x.PurchaseDate, from, to))
            .ToList();

        var result = new InventoryReportDto
        {
            TotalParts = parts.Count,
            ActiveParts = activeParts.Count,
            LowStockParts = activeParts.Count(x => x.Stock <= x.MinimumStock),
            OutOfStockParts = activeParts.Count(x => x.Stock == 0),
            EstimatedInventoryValue = activeParts.Sum(x => x.Stock * x.UnitPrice),
            PurchasesCount = filteredPurchases.Count,
            PurchasesAmount = filteredPurchases.Sum(x => x.Total)
        };

        return Result<InventoryReportDto>.Success(result);
    }

    public async Task<Result<MechanicsReportDto>> GetMechanicsReportAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(from, to))
        {
            return Result<MechanicsReportDto>.Failure(ReportErrors.DateRangeInvalid);
        }

        var roles = await _unitOfWork.Repository<Role>().GetAllAsync(cancellationToken);
        var mechanicRoleId = roles
            .Where(x => IsName(x.RoleName, MechanicRoleName))
            .Select(x => (int?)x.RoleId)
            .FirstOrDefault();

        var personRoles = await _unitOfWork.Repository<PersonRole>().GetAllAsync(cancellationToken);
        var totalMechanics = mechanicRoleId.HasValue
            ? personRoles.Where(x => x.RoleId == mechanicRoleId.Value).Select(x => x.PersonId).Distinct().Count()
            : 0;
        var activeMechanics = mechanicRoleId.HasValue
            ? personRoles.Where(x => x.RoleId == mechanicRoleId.Value && x.IsActive).Select(x => x.PersonId).Distinct().Count()
            : 0;

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().GetAllAsync(cancellationToken);
        var filteredServiceOrderIds = serviceOrders
            .Where(x => IsWithinRange(x.EntryDate, from, to))
            .Select(x => x.ServiceOrderId)
            .ToHashSet();

        var orderServices = await _unitOfWork.Repository<OrderService>().GetAllAsync(cancellationToken);
        var filteredOrderServices = filteredServiceOrderIds.Count == 0
            ? Array.Empty<OrderService>()
            : orderServices.Where(x => filteredServiceOrderIds.Contains(x.ServiceOrderId)).ToArray();

        var filteredOrderServiceIds = filteredOrderServices
            .Select(x => x.OrderServiceId)
            .ToHashSet();

        var assignments = await _unitOfWork.Repository<MechanicAssignment>().GetAllAsync(cancellationToken);
        var totalAssignments = filteredOrderServiceIds.Count == 0
            ? 0
            : assignments.Count(x => filteredOrderServiceIds.Contains(x.OrderServiceId));

        var result = new MechanicsReportDto
        {
            TotalMechanics = totalMechanics,
            ActiveMechanics = activeMechanics,
            TotalAssignments = totalAssignments,
            ServicesWithWorkPerformed = filteredOrderServices.Count(x => !string.IsNullOrWhiteSpace(x.WorkPerformed)),
            ServicesPendingWorkPerformed = filteredOrderServices.Count(x => string.IsNullOrWhiteSpace(x.WorkPerformed))
        };

        return Result<MechanicsReportDto>.Success(result);
    }

    public async Task<Result<ServiceOrdersReportDto>> GetServiceOrdersReportAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(from, to))
        {
            return Result<ServiceOrdersReportDto>.Failure(ReportErrors.DateRangeInvalid);
        }

        var serviceOrders = await _unitOfWork.Repository<ServiceOrder>().GetAllAsync(cancellationToken);
        var filteredOrders = serviceOrders
            .Where(x => IsWithinRange(x.EntryDate, from, to))
            .ToList();

        var statuses = await _unitOfWork.Repository<OrderStatus>().GetAllAsync(cancellationToken);
        var pendingStatusIds = GetStatusIds(statuses, PendingOrderStatusName);
        var inProgressStatusIds = GetStatusIds(statuses, InProgressOrderStatusName);
        var completedStatusIds = GetStatusIds(statuses, CompletedOrderStatusName);
        var cancelledStatusIds = GetStatusIds(statuses, CancelledOrderStatusName);
        var voidedStatusIds = GetStatusIds(statuses, VoidedOrderStatusName);

        var result = new ServiceOrdersReportDto
        {
            TotalOrders = filteredOrders.Count,
            PendingOrders = filteredOrders.Count(x => pendingStatusIds.Contains(x.OrderStatusId)),
            InProgressOrders = filteredOrders.Count(x => inProgressStatusIds.Contains(x.OrderStatusId)),
            CompletedOrders = filteredOrders.Count(x => completedStatusIds.Contains(x.OrderStatusId)),
            CancelledOrders = filteredOrders.Count(x => cancelledStatusIds.Contains(x.OrderStatusId)),
            VoidedOrders = filteredOrders.Count(x => voidedStatusIds.Contains(x.OrderStatusId))
        };

        return Result<ServiceOrdersReportDto>.Success(result);
    }

    public async Task<Result<PaymentsReportDto>> GetPaymentsReportAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!IsDateRangeValid(from, to))
        {
            return Result<PaymentsReportDto>.Failure(ReportErrors.DateRangeInvalid);
        }

        var payments = await _unitOfWork.Repository<Payment>().GetAllAsync(cancellationToken);
        var filteredPayments = payments
            .Where(x => IsWithinRange(x.PaymentDate, from, to))
            .ToList();

        var statuses = await _unitOfWork.Repository<PaymentStatus>().GetAllAsync(cancellationToken);
        var completedStatusIds = GetStatusIds(statuses, CompletedPaymentStatusName);
        var refundedStatusIds = GetStatusIds(statuses, RefundedPaymentStatusName);

        var totalAmount = filteredPayments.Sum(x => x.Amount);
        var completedAmount = filteredPayments
            .Where(x => completedStatusIds.Contains(x.PaymentStatusId))
            .Sum(x => x.Amount);
        var refundedAmount = filteredPayments
            .Where(x => refundedStatusIds.Contains(x.PaymentStatusId))
            .Sum(x => x.Amount);
        var pendingOrOtherAmount = totalAmount - completedAmount - refundedAmount;

        var result = new PaymentsReportDto
        {
            TotalPayments = filteredPayments.Count,
            TotalAmount = totalAmount,
            CompletedAmount = completedAmount,
            RefundedAmount = refundedAmount,
            PendingOrOtherAmount = pendingOrOtherAmount
        };

        return Result<PaymentsReportDto>.Success(result);
    }

    private static bool IsDateRangeValid(DateTime? from, DateTime? to)
    {
        return !from.HasValue || !to.HasValue || from.Value <= to.Value;
    }

    private static bool IsWithinRange(DateTime value, DateTime? from, DateTime? to)
    {
        if (from.HasValue && value < from.Value)
        {
            return false;
        }

        if (to.HasValue && value > to.Value)
        {
            return false;
        }

        return true;
    }

    private static bool IsName(string value, string expected)
    {
        return value.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static HashSet<int> GetStatusIds<TStatus>(IReadOnlyList<TStatus> statuses, string statusName)
        where TStatus : class
    {
        if (typeof(TStatus) == typeof(OrderStatus))
        {
            return statuses
                .Cast<OrderStatus>()
                .Where(x => IsName(x.Name, statusName))
                .Select(x => x.OrderStatusId)
                .ToHashSet();
        }

        return statuses
            .Cast<PaymentStatus>()
            .Where(x => IsName(x.Name, statusName))
            .Select(x => x.PaymentStatusId)
            .ToHashSet();
    }
}
