using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.InvoiceDetails.Dtos;
using Application.Features.InvoiceDetails.Errors;
using Application.Features.InvoiceDetails.Requests;
using Domain.Entities;

namespace Application.Features.InvoiceDetails;

public class InvoiceDetailService : IInvoiceDetailService
{
    private const int ConceptMaxLength = 150;
    private const int LineTypeMaxLength = 20;
    private const string PartLineType = "part";
    private const string LaborLineType = "labor";

    private readonly IUnitOfWork _unitOfWork;

    public InvoiceDetailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<InvoiceDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var invoiceDetails = await invoiceDetailRepository.GetAllAsync(cancellationToken);

        var invoiceDetailDtos = invoiceDetails
            .OrderBy(x => x.InvoiceDetailId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<InvoiceDetailDto>>.Success(invoiceDetailDtos);
    }

    public async Task<Result<InvoiceDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var invoiceDetail = await invoiceDetailRepository.GetByIdAsync(id, cancellationToken);

        if (invoiceDetail is null)
        {
            return Result<InvoiceDetailDto>.Failure(InvoiceDetailErrors.NotFound);
        }

        return Result<InvoiceDetailDto>.Success(MapToDto(invoiceDetail));
    }

    public async Task<Result<InvoiceDetailsByInvoiceDto>> GetByInvoiceIdAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return Result<InvoiceDetailsByInvoiceDto>.Failure(InvoiceDetailErrors.InvoiceIdInvalid);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result<InvoiceDetailsByInvoiceDto>.Failure(InvoiceDetailErrors.InvoiceNotFound);
        }

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var invoiceDetails = await invoiceDetailRepository.FindAsync(
            x => x.InvoiceId == invoiceId,
            cancellationToken);

        var result = new InvoiceDetailsByInvoiceDto
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceStatusId = invoice.InvoiceStatusId,
            Subtotal = invoice.Subtotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Details = invoiceDetails
                .OrderBy(x => x.InvoiceDetailId)
                .Select(MapToLineDto)
                .ToList()
        };

        return Result<InvoiceDetailsByInvoiceDto>.Success(result);
    }

    public async Task<Result<InvoiceDetailDto>> CreateAsync(
        CreateInvoiceDetailRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoiceId = request?.InvoiceId ?? 0;
        var sourcePartId = request?.SourcePartId;
        var concept = NormalizeRequiredText(request?.Concept);
        var quantity = request?.Quantity ?? 0;
        var unitPrice = request?.UnitPrice ?? 0m;
        var lineType = NormalizeLineType(request?.LineType);

        var validationError = Validate(invoiceId, sourcePartId, concept, quantity, unitPrice, lineType);
        if (validationError is not null)
        {
            return Result<InvoiceDetailDto>.Failure(validationError);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result<InvoiceDetailDto>.Failure(InvoiceDetailErrors.InvoiceNotFound);
        }

        var sourcePartValidationError = await ValidateSourcePartAsync(sourcePartId, lineType, cancellationToken);
        if (sourcePartValidationError is not null)
        {
            return Result<InvoiceDetailDto>.Failure(sourcePartValidationError);
        }

        if (lineType == LaborLineType)
        {
            sourcePartId = null;
        }

        var subtotal = CalculateSubtotal(quantity, unitPrice);

        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var existingDetails = await invoiceDetailRepository.FindAsync(
            x => x.InvoiceId == invoiceId,
            cancellationToken);

        invoice.Subtotal = existingDetails.Sum(x => x.Subtotal) + subtotal;
        invoice.Total = invoice.Subtotal + invoice.Tax;

        var invoiceDetail = new InvoiceDetail
        {
            InvoiceId = invoiceId,
            SourcePartId = sourcePartId,
            Concept = concept,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal,
            LineType = lineType
        };

        await invoiceDetailRepository.AddAsync(invoiceDetail, cancellationToken);
        invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDetailDto>.Success(MapToDto(invoiceDetail));
    }

    public async Task<Result<InvoiceDetailDto>> UpdateAsync(
        int id,
        UpdateInvoiceDetailRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var invoiceDetail = await invoiceDetailRepository.GetByIdAsync(id, cancellationToken);

        if (invoiceDetail is null)
        {
            return Result<InvoiceDetailDto>.Failure(InvoiceDetailErrors.NotFound);
        }

        var newInvoiceId = request?.InvoiceId ?? 0;
        var newSourcePartId = request?.SourcePartId;
        var newConcept = NormalizeRequiredText(request?.Concept);
        var newQuantity = request?.Quantity ?? 0;
        var newUnitPrice = request?.UnitPrice ?? 0m;
        var newLineType = NormalizeLineType(request?.LineType);

        var validationError = Validate(newInvoiceId, newSourcePartId, newConcept, newQuantity, newUnitPrice, newLineType);
        if (validationError is not null)
        {
            return Result<InvoiceDetailDto>.Failure(validationError);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var newInvoice = await invoiceRepository.GetByIdAsync(newInvoiceId, cancellationToken);

        if (newInvoice is null)
        {
            return Result<InvoiceDetailDto>.Failure(InvoiceDetailErrors.InvoiceNotFound);
        }

        var sourcePartValidationError = await ValidateSourcePartAsync(newSourcePartId, newLineType, cancellationToken);
        if (sourcePartValidationError is not null)
        {
            return Result<InvoiceDetailDto>.Failure(sourcePartValidationError);
        }

        if (newLineType == LaborLineType)
        {
            newSourcePartId = null;
        }

        var oldInvoiceId = invoiceDetail.InvoiceId;
        var oldSubtotal = invoiceDetail.Subtotal;
        var newSubtotal = CalculateSubtotal(newQuantity, newUnitPrice);

        if (newInvoiceId == oldInvoiceId)
        {
            var sameInvoiceDetails = await invoiceDetailRepository.FindAsync(
                x => x.InvoiceId == newInvoiceId,
                cancellationToken);

            newInvoice.Subtotal = sameInvoiceDetails.Sum(x => x.Subtotal) - oldSubtotal + newSubtotal;
            newInvoice.Total = newInvoice.Subtotal + newInvoice.Tax;
            invoiceRepository.Update(newInvoice);
        }
        else
        {
            var oldInvoice = await invoiceRepository.GetByIdAsync(oldInvoiceId, cancellationToken);
            if (oldInvoice is null)
            {
                return Result<InvoiceDetailDto>.Failure(InvoiceDetailErrors.InvoiceNotFound);
            }

            var oldInvoiceDetails = await invoiceDetailRepository.FindAsync(
                x => x.InvoiceId == oldInvoiceId,
                cancellationToken);

            oldInvoice.Subtotal = oldInvoiceDetails
                .Where(x => x.InvoiceDetailId != id)
                .Sum(x => x.Subtotal);
            oldInvoice.Total = oldInvoice.Subtotal + oldInvoice.Tax;
            invoiceRepository.Update(oldInvoice);

            var newInvoiceDetails = await invoiceDetailRepository.FindAsync(
                x => x.InvoiceId == newInvoiceId,
                cancellationToken);

            newInvoice.Subtotal = newInvoiceDetails.Sum(x => x.Subtotal) + newSubtotal;
            newInvoice.Total = newInvoice.Subtotal + newInvoice.Tax;
            invoiceRepository.Update(newInvoice);
        }

        invoiceDetail.InvoiceId = newInvoiceId;
        invoiceDetail.SourcePartId = newSourcePartId;
        invoiceDetail.Concept = newConcept;
        invoiceDetail.Quantity = newQuantity;
        invoiceDetail.UnitPrice = newUnitPrice;
        invoiceDetail.Subtotal = newSubtotal;
        invoiceDetail.LineType = newLineType;

        invoiceDetailRepository.Update(invoiceDetail);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDetailDto>.Success(MapToDto(invoiceDetail));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoiceDetailRepository = _unitOfWork.Repository<InvoiceDetail>();
        var invoiceDetail = await invoiceDetailRepository.GetByIdAsync(id, cancellationToken);

        if (invoiceDetail is null)
        {
            return Result.Failure(InvoiceDetailErrors.NotFound);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepository.GetByIdAsync(invoiceDetail.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure(InvoiceDetailErrors.InvoiceNotFound);
        }

        var invoiceDetails = await invoiceDetailRepository.FindAsync(
            x => x.InvoiceId == invoiceDetail.InvoiceId,
            cancellationToken);

        invoice.Subtotal = invoiceDetails
            .Where(x => x.InvoiceDetailId != id)
            .Sum(x => x.Subtotal);
        invoice.Total = invoice.Subtotal + invoice.Tax;

        invoiceDetailRepository.Remove(invoiceDetail);
        invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static InvoiceDetailDto MapToDto(InvoiceDetail invoiceDetail)
    {
        return new InvoiceDetailDto
        {
            InvoiceDetailId = invoiceDetail.InvoiceDetailId,
            InvoiceId = invoiceDetail.InvoiceId,
            SourcePartId = invoiceDetail.SourcePartId,
            Concept = invoiceDetail.Concept,
            Quantity = invoiceDetail.Quantity,
            UnitPrice = invoiceDetail.UnitPrice,
            Subtotal = invoiceDetail.Subtotal,
            LineType = invoiceDetail.LineType
        };
    }

    private static InvoiceDetailLineDto MapToLineDto(InvoiceDetail invoiceDetail)
    {
        return new InvoiceDetailLineDto
        {
            InvoiceDetailId = invoiceDetail.InvoiceDetailId,
            SourcePartId = invoiceDetail.SourcePartId,
            Concept = invoiceDetail.Concept,
            Quantity = invoiceDetail.Quantity,
            UnitPrice = invoiceDetail.UnitPrice,
            Subtotal = invoiceDetail.Subtotal,
            LineType = invoiceDetail.LineType
        };
    }

    private static Error? Validate(
        int invoiceId,
        int? sourcePartId,
        string concept,
        int quantity,
        decimal unitPrice,
        string lineType)
    {
        if (invoiceId <= 0)
        {
            return InvoiceDetailErrors.InvoiceIdInvalid;
        }

        if (sourcePartId.HasValue && sourcePartId.Value <= 0)
        {
            return InvoiceDetailErrors.SourcePartIdInvalid;
        }

        if (string.IsNullOrWhiteSpace(concept))
        {
            return InvoiceDetailErrors.ConceptRequired;
        }

        if (concept.Length > ConceptMaxLength)
        {
            return InvoiceDetailErrors.ConceptTooLong;
        }

        if (quantity <= 0)
        {
            return InvoiceDetailErrors.QuantityInvalid;
        }

        if (unitPrice < 0m)
        {
            return InvoiceDetailErrors.UnitPriceInvalid;
        }

        if (string.IsNullOrWhiteSpace(lineType))
        {
            return InvoiceDetailErrors.LineTypeRequired;
        }

        if (lineType.Length > LineTypeMaxLength)
        {
            return InvoiceDetailErrors.LineTypeTooLong;
        }

        if (lineType != PartLineType && lineType != LaborLineType)
        {
            return InvoiceDetailErrors.LineTypeInvalid;
        }

        return null;
    }

    private async Task<Error?> ValidateSourcePartAsync(int? sourcePartId, string lineType, CancellationToken cancellationToken)
    {
        if (lineType == PartLineType && !sourcePartId.HasValue)
        {
            return InvoiceDetailErrors.SourcePartRequiredForPartLine;
        }

        if (!sourcePartId.HasValue)
        {
            return null;
        }

        var partRepository = _unitOfWork.Repository<Part>();
        var partExists = await partRepository.ExistsAsync(
            x => x.PartId == sourcePartId.Value,
            cancellationToken);

        if (!partExists)
        {
            return InvoiceDetailErrors.SourcePartNotFound;
        }

        return null;
    }

    private static string NormalizeRequiredText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string NormalizeLineType(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static decimal CalculateSubtotal(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }
}
