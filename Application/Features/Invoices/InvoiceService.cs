using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Invoices.Dtos;
using Application.Features.Invoices.Errors;
using Application.Features.Invoices.Requests;
using Domain.Entities;

namespace Application.Features.Invoices;

public class InvoiceService : IInvoiceService
{
    private const int InvoiceNumberMaxLength = 50;
    private const string CancelledStatusName = "Cancelled";
    private const string VoidedStatusName = "Voided";

    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoices = await invoiceRepository.GetAllAsync(cancellationToken);

        var invoiceDtos = invoices
            .OrderBy(x => x.InvoiceId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<InvoiceDto>>.Success(invoiceDtos);
    }

    public async Task<Result<InvoiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.NotFound);
        }

        return Result<InvoiceDto>.Success(MapToDto(invoice));
    }

    public async Task<Result<InvoiceDto>> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoiceRepository = _unitOfWork.Repository<Invoice>();

        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var invoiceStatusId = request?.InvoiceStatusId ?? 0;
        var invoiceDate = request?.InvoiceDate ?? DateTime.UtcNow;
        var tax = request?.Tax ?? 0m;
        var observations = NormalizeOptionalText(request?.Observations);
        var requestedInvoiceNumber = NormalizeOptionalText(request?.InvoiceNumber)?.ToUpperInvariant();

        var validationError = Validate(serviceOrderId, invoiceStatusId, invoiceDate, tax);
        if (validationError is not null)
        {
            return Result<InvoiceDto>.Failure(validationError);
        }

        if (requestedInvoiceNumber is not null && requestedInvoiceNumber.Length > InvoiceNumberMaxLength)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.InvoiceNumberTooLong);
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.ServiceOrderNotFound);
        }

        var canInvoiceServiceOrder = await CanInvoiceServiceOrderAsync(serviceOrder, cancellationToken);
        if (!canInvoiceServiceOrder)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.ServiceOrderCannotBeInvoicedConflict);
        }

        var serviceOrderAlreadyHasInvoice = await invoiceRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId,
            cancellationToken);

        if (serviceOrderAlreadyHasInvoice)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.ServiceOrderAlreadyHasInvoiceConflict);
        }

        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var invoiceStatusExists = await invoiceStatusRepository.ExistsAsync(
            x => x.InvoiceStatusId == invoiceStatusId,
            cancellationToken);

        if (!invoiceStatusExists)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.InvoiceStatusNotFound);
        }

        string invoiceNumber;
        if (requestedInvoiceNumber is null)
        {
            invoiceNumber = await GenerateUniqueInvoiceNumberAsync(invoiceRepository, cancellationToken);
        }
        else
        {
            var invoiceNumberAlreadyExists = await invoiceRepository.ExistsAsync(
                x => x.InvoiceNumber == requestedInvoiceNumber,
                cancellationToken);

            if (invoiceNumberAlreadyExists)
            {
                return Result<InvoiceDto>.Failure(InvoiceErrors.InvoiceNumberAlreadyExists);
            }

            invoiceNumber = requestedInvoiceNumber;
        }

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            ServiceOrderId = serviceOrderId,
            InvoiceStatusId = invoiceStatusId,
            InvoiceDate = invoiceDate,
            Subtotal = 0m,
            Tax = tax,
            Total = tax,
            Observations = observations
        };

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Success(MapToDto(invoice));
    }

    public async Task<Result<InvoiceDto>> UpdateAsync(int id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.NotFound);
        }

        var serviceOrderId = request?.ServiceOrderId ?? 0;
        var invoiceStatusId = request?.InvoiceStatusId ?? 0;
        var invoiceDate = request?.InvoiceDate ?? default;
        var tax = request?.Tax ?? 0m;
        var observations = NormalizeOptionalText(request?.Observations);
        var requestedInvoiceNumber = NormalizeOptionalText(request?.InvoiceNumber)?.ToUpperInvariant();

        var validationError = Validate(serviceOrderId, invoiceStatusId, invoiceDate, tax);
        if (validationError is not null)
        {
            return Result<InvoiceDto>.Failure(validationError);
        }

        string invoiceNumber = invoice.InvoiceNumber;
        if (requestedInvoiceNumber is not null)
        {
            if (requestedInvoiceNumber.Length > InvoiceNumberMaxLength)
            {
                return Result<InvoiceDto>.Failure(InvoiceErrors.InvoiceNumberTooLong);
            }

            var invoiceNumberAlreadyExists = await invoiceRepository.ExistsAsync(
                x => x.InvoiceNumber == requestedInvoiceNumber && x.InvoiceId != id,
                cancellationToken);

            if (invoiceNumberAlreadyExists)
            {
                return Result<InvoiceDto>.Failure(InvoiceErrors.InvoiceNumberAlreadyExists);
            }

            invoiceNumber = requestedInvoiceNumber;
        }

        var serviceOrderRepository = _unitOfWork.Repository<ServiceOrder>();
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(serviceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.ServiceOrderNotFound);
        }

        var canInvoiceServiceOrder = await CanInvoiceServiceOrderAsync(serviceOrder, cancellationToken);
        if (!canInvoiceServiceOrder)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.ServiceOrderCannotBeInvoicedConflict);
        }

        var serviceOrderAlreadyHasAnotherInvoice = await invoiceRepository.ExistsAsync(
            x => x.ServiceOrderId == serviceOrderId && x.InvoiceId != id,
            cancellationToken);

        if (serviceOrderAlreadyHasAnotherInvoice)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.ServiceOrderAlreadyHasInvoiceConflict);
        }

        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var invoiceStatusExists = await invoiceStatusRepository.ExistsAsync(
            x => x.InvoiceStatusId == invoiceStatusId,
            cancellationToken);

        if (!invoiceStatusExists)
        {
            return Result<InvoiceDto>.Failure(InvoiceErrors.InvoiceStatusNotFound);
        }

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var invoiceDetails = await invoiceDetailRepository.FindAsync(
            x => x.InvoiceId == id,
            cancellationToken);

        var subtotal = invoiceDetails.Sum(x => x.Subtotal);
        var total = subtotal + tax;

        invoice.InvoiceNumber = invoiceNumber;
        invoice.ServiceOrderId = serviceOrderId;
        invoice.InvoiceStatusId = invoiceStatusId;
        invoice.InvoiceDate = invoiceDate;
        invoice.Subtotal = subtotal;
        invoice.Tax = tax;
        invoice.Total = total;
        invoice.Observations = observations;

        invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Success(MapToDto(invoice));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure(InvoiceErrors.NotFound);
        }

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var inUseByInvoiceDetails = await invoiceDetailRepository.ExistsAsync(
            x => x.InvoiceId == id,
            cancellationToken);

        if (inUseByInvoiceDetails)
        {
            return Result.Failure(InvoiceErrors.InUse);
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        var inUseByPayments = await paymentRepository.ExistsAsync(
            x => x.InvoiceId == id,
            cancellationToken);

        if (inUseByPayments)
        {
            return Result.Failure(InvoiceErrors.InUse);
        }

        invoiceRepository.Remove(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            ServiceOrderId = invoice.ServiceOrderId,
            InvoiceStatusId = invoice.InvoiceStatusId,
            InvoiceDate = invoice.InvoiceDate,
            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Observations = invoice.Observations
        };
    }

    private static Error? Validate(int serviceOrderId, int invoiceStatusId, DateTime invoiceDate, decimal tax)
    {
        if (serviceOrderId <= 0)
        {
            return InvoiceErrors.ServiceOrderIdInvalid;
        }

        if (invoiceStatusId <= 0)
        {
            return InvoiceErrors.InvoiceStatusIdInvalid;
        }

        if (invoiceDate == default)
        {
            return InvoiceErrors.InvoiceDateInvalid;
        }

        if (tax < 0m)
        {
            return InvoiceErrors.TaxInvalid;
        }

        return null;
    }

    private async Task<bool> CanInvoiceServiceOrderAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken)
    {
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

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
