using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.InvoiceBusiness.Dtos;
using Application.Features.InvoiceBusiness.Errors;
using Application.Features.InvoiceBusiness.Requests;
using Domain.Entities;

namespace Application.Features.InvoiceBusiness;

public class InvoiceBusinessService : IInvoiceBusinessService
{
    private const int InvoiceNumberMaxLength = 50;

    private const string CancelledOrderStatusName = "Cancelled";
    private const string VoidedOrderStatusName = "Voided";

    private const string DraftInvoiceStatusName = "Draft";
    private const string IssuedInvoiceStatusName = "Issued";
    private const string CancelledInvoiceStatusName = "Cancelled";

    private const string CompletedPaymentStatusName = "Completed";
    private const string LaborLineType = "labor";
    private const string PartLineType = "part";

    private readonly IUnitOfWork _unitOfWork;

    public InvoiceBusinessService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GeneratedInvoiceDto>> GenerateFromServiceOrderAsync(
        int serviceOrderId,
        GenerateInvoiceFromServiceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.ServiceOrderIdInvalid);
        }

        var tax = request?.Tax ?? 0m;
        if (tax < 0m)
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.TaxInvalid);
        }

        var invoiceNumberInput = NormalizeOptionalText(request?.InvoiceNumber)?.ToUpperInvariant();
        var observations = NormalizeOptionalText(request?.Observations);

        if (invoiceNumberInput is not null && invoiceNumberInput.Length > InvoiceNumberMaxLength)
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.InvoiceNumberTooLong);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);
        if (serviceOrder is null)
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.ServiceOrderNotFound);
        }

        var orderStatus = await _unitOfWork.Repository<OrderStatus>().GetByIdAsync(serviceOrder.OrderStatusId, cancellationToken);
        if (orderStatus is null ||
            IsStatusName(orderStatus.Name, CancelledOrderStatusName) ||
            IsStatusName(orderStatus.Name, VoidedOrderStatusName))
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.ServiceOrderCannotBeInvoicedConflict);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var existingInvoice = await invoiceRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (existingInvoice)
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.ServiceOrderAlreadyHasInvoiceConflict);
        }

        string invoiceNumber;
        if (invoiceNumberInput is null)
        {
            invoiceNumber = await GenerateUniqueInvoiceNumberAsync(invoiceRepository, cancellationToken);
        }
        else
        {
            var duplicateInvoiceNumber = await invoiceRepository.ExistsAsync(
                x => x.InvoiceNumber == invoiceNumberInput,
                cancellationToken);

            if (duplicateInvoiceNumber)
            {
                return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.InvoiceNumberAlreadyExists);
            }

            invoiceNumber = invoiceNumberInput;
        }

        var invoiceStatusIdFromRequest = request?.InvoiceStatusId;
        int invoiceStatusId;

        if (invoiceStatusIdFromRequest.HasValue)
        {
            if (invoiceStatusIdFromRequest.Value <= 0)
            {
                return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.InvoiceStatusIdInvalid);
            }

            var status = await _unitOfWork.Repository<InvoiceStatus>().GetByIdAsync(invoiceStatusIdFromRequest.Value, cancellationToken);
            if (status is null)
            {
                return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.InvoiceStatusNotFound);
            }

            invoiceStatusId = status.InvoiceStatusId;
        }
        else
        {
            var invoiceStatuses = await _unitOfWork.Repository<InvoiceStatus>().GetAllAsync(cancellationToken);
            var draftStatus = invoiceStatuses.FirstOrDefault(x => IsStatusName(x.Name, DraftInvoiceStatusName));
            if (draftStatus is null)
            {
                return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.DraftStatusNotFound);
            }

            invoiceStatusId = draftStatus.InvoiceStatusId;
        }

        var orderServices = await _unitOfWork.Repository<OrderService>().FindAsync(
            x => x.ServiceOrderId == serviceOrderId && x.CustomerApproved == true,
            cancellationToken);

        var orderServiceIds = orderServices
            .Select(x => x.OrderServiceId)
            .Distinct()
            .ToList();

        var approvedParts = orderServiceIds.Count == 0
            ? Array.Empty<OrderServicePart>()
            : (await _unitOfWork.Repository<OrderServicePart>().FindAsync(
                x => orderServiceIds.Contains(x.OrderServiceId) && x.CustomerApproved == true,
                cancellationToken)).ToArray();

        if (orderServices.Count == 0 && approvedParts.Length == 0)
        {
            return Result<GeneratedInvoiceDto>.Failure(InvoiceBusinessErrors.NoBillableItemsConflict);
        }

        var partIds = approvedParts
            .Select(x => x.PartId)
            .Distinct()
            .ToList();

        var partById = partIds.Count == 0
            ? new Dictionary<int, Part>()
            : (await _unitOfWork.Repository<Part>().FindAsync(x => partIds.Contains(x.PartId), cancellationToken))
                .ToDictionary(x => x.PartId, x => x);

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            ServiceOrderId = serviceOrderId,
            InvoiceStatusId = invoiceStatusId,
            InvoiceDate = DateTime.UtcNow,
            Tax = tax,
            Subtotal = 0m,
            Total = 0m,
            Observations = observations
        };

        await invoiceRepository.AddAsync(invoice, cancellationToken);

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var createdDetails = new List<InvoiceDetail>();
        decimal subtotal = 0m;

        foreach (var orderService in orderServices.OrderBy(x => x.OrderServiceId))
        {
            var concept = NormalizeOptionalText(orderService.Description)
                ?? NormalizeOptionalText(orderService.WorkPerformed)
                ?? $"Labor service {orderService.OrderServiceId}";

            var detail = new InvoiceDetail
            {
                Invoice = invoice,
                SourcePartId = null,
                Concept = concept,
                Quantity = 1,
                UnitPrice = orderService.LaborCost,
                Subtotal = orderService.LaborCost,
                LineType = LaborLineType
            };

            subtotal += detail.Subtotal;
            createdDetails.Add(detail);
            await invoiceDetailRepository.AddAsync(detail, cancellationToken);
        }

        foreach (var orderServicePart in approvedParts.OrderBy(x => x.OrderServicePartId))
        {
            var concept = partById.TryGetValue(orderServicePart.PartId, out var part)
                ? part.Description
                : $"Part {orderServicePart.PartId}";

            var detailSubtotal = orderServicePart.Quantity * orderServicePart.AppliedUnitPrice;
            var detail = new InvoiceDetail
            {
                Invoice = invoice,
                SourcePartId = orderServicePart.PartId,
                Concept = concept,
                Quantity = orderServicePart.Quantity,
                UnitPrice = orderServicePart.AppliedUnitPrice,
                Subtotal = detailSubtotal,
                LineType = PartLineType
            };

            subtotal += detailSubtotal;
            createdDetails.Add(detail);
            await invoiceDetailRepository.AddAsync(detail, cancellationToken);
        }

        invoice.Subtotal = subtotal;
        invoice.Total = subtotal + tax;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<GeneratedInvoiceDto>.Success(new GeneratedInvoiceDto
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ServiceOrderId = invoice.ServiceOrderId,
            InvoiceStatusId = invoice.InvoiceStatusId,
            InvoiceDate = invoice.InvoiceDate,
            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Observations = invoice.Observations,
            Details = createdDetails
                .OrderBy(x => x.InvoiceDetailId)
                .Select(MapGeneratedDetail)
                .ToList()
        });
    }

    public async Task<Result<InvoiceBusinessResultDto>> RecalculateAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceIdInvalid);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceNotFound);
        }

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var details = await invoiceDetailRepository.FindAsync(x => x.InvoiceId == invoiceId, cancellationToken);

        foreach (var detail in details)
        {
            detail.Subtotal = detail.Quantity * detail.UnitPrice;
            invoiceDetailRepository.Update(detail);
        }

        invoice.Subtotal = details.Sum(x => x.Subtotal);
        invoice.Total = invoice.Subtotal + invoice.Tax;
        invoiceRepository.Update(invoice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceBusinessResultDto>.Success(MapResult(invoice, "Recalculate"));
    }

    public async Task<Result<InvoiceBusinessResultDto>> IssueAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceIdInvalid);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceNotFound);
        }

        var invoiceStatuses = await _unitOfWork.Repository<InvoiceStatus>().GetAllAsync(cancellationToken);
        var issuedStatus = invoiceStatuses.FirstOrDefault(x => IsStatusName(x.Name, IssuedInvoiceStatusName));
        if (issuedStatus is null)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.IssuedStatusNotFound);
        }

        if (invoice.Total <= 0m)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceTotalInvalid);
        }

        invoice.InvoiceStatusId = issuedStatus.InvoiceStatusId;
        invoiceRepository.Update(invoice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceBusinessResultDto>.Success(MapResult(invoice, "Issue"));
    }

    public async Task<Result<InvoiceBusinessResultDto>> CancelAsync(
        int invoiceId,
        CancelInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceIdInvalid);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.InvoiceNotFound);
        }

        var reason = NormalizeOptionalText(request?.Reason);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.CancelReasonRequired);
        }

        var invoiceStatuses = await _unitOfWork.Repository<InvoiceStatus>().GetAllAsync(cancellationToken);
        var cancelledStatus = invoiceStatuses.FirstOrDefault(x => IsStatusName(x.Name, CancelledInvoiceStatusName));
        if (cancelledStatus is null)
        {
            return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.CancelledStatusNotFound);
        }

        var completedStatusIds = (await _unitOfWork.Repository<PaymentStatus>().GetAllAsync(cancellationToken))
            .Where(x => IsStatusName(x.Name, CompletedPaymentStatusName))
            .Select(x => x.PaymentStatusId)
            .Distinct()
            .ToArray();

        if (completedStatusIds.Length > 0)
        {
            var hasCompletedPayments = await _unitOfWork.Repository<Payment>().ExistsAsync(
                x => x.InvoiceId == invoiceId && completedStatusIds.Contains(x.PaymentStatusId),
                cancellationToken);

            if (hasCompletedPayments)
            {
                return Result<InvoiceBusinessResultDto>.Failure(InvoiceBusinessErrors.CompletedPaymentsExistConflict);
            }
        }

        var cancellationNote = $"Cancelled: {reason}";
        invoice.Observations = string.IsNullOrWhiteSpace(invoice.Observations)
            ? cancellationNote
            : $"{invoice.Observations}{Environment.NewLine}{cancellationNote}";

        invoice.InvoiceStatusId = cancelledStatus.InvoiceStatusId;
        invoiceRepository.Update(invoice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceBusinessResultDto>.Success(MapResult(invoice, "Cancel"));
    }

    private async Task<string> GenerateUniqueInvoiceNumberAsync(
        IGenericRepository<Invoice> invoiceRepository,
        CancellationToken cancellationToken)
    {
        var baseValue = $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var candidate = baseValue;
        var counter = 1;

        while (await invoiceRepository.ExistsAsync(x => x.InvoiceNumber == candidate, cancellationToken))
        {
            candidate = $"{baseValue}-{counter}";
            counter++;
        }

        if (candidate.Length > InvoiceNumberMaxLength)
        {
            candidate = candidate[..InvoiceNumberMaxLength];
        }

        return candidate;
    }

    private static GeneratedInvoiceDetailDto MapGeneratedDetail(InvoiceDetail detail)
    {
        return new GeneratedInvoiceDetailDto
        {
            InvoiceDetailId = detail.InvoiceDetailId,
            SourcePartId = detail.SourcePartId,
            Concept = detail.Concept,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            Subtotal = detail.Subtotal,
            LineType = detail.LineType
        };
    }

    private static InvoiceBusinessResultDto MapResult(Invoice invoice, string action)
    {
        return new InvoiceBusinessResultDto
        {
            InvoiceId = invoice.InvoiceId,
            Action = action,
            Success = true,
            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total
        };
    }

    private static bool IsStatusName(string? value, string expected)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
