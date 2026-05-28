using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.InvoiceStatuses.Dtos;
using Application.Features.InvoiceStatuses.Errors;
using Application.Features.InvoiceStatuses.Requests;
using Domain.Entities;

namespace Application.Features.InvoiceStatuses;

public class InvoiceStatusService : IInvoiceStatusService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceStatusService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<InvoiceStatusDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var invoiceStatuses = await invoiceStatusRepository.GetAllAsync(cancellationToken);

        var invoiceStatusDtos = invoiceStatuses
            .OrderBy(x => x.InvoiceStatusId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<InvoiceStatusDto>>.Success(invoiceStatusDtos);
    }

    public async Task<Result<InvoiceStatusDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var invoiceStatus = await invoiceStatusRepository.GetByIdAsync(id, cancellationToken);

        if (invoiceStatus is null)
        {
            return Result<InvoiceStatusDto>.Failure(InvoiceStatusErrors.NotFound);
        }

        return Result<InvoiceStatusDto>.Success(MapToDto(invoiceStatus));
    }

    public async Task<Result<InvoiceStatusDto>> CreateAsync(
        CreateInvoiceStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<InvoiceStatusDto>.Failure(validationError);
        }

        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var nameAlreadyExists = await invoiceStatusRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<InvoiceStatusDto>.Failure(InvoiceStatusErrors.NameAlreadyExists);
        }

        var invoiceStatus = new InvoiceStatus
        {
            Name = normalizedName
        };

        await invoiceStatusRepository.AddAsync(invoiceStatus, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceStatusDto>.Success(MapToDto(invoiceStatus));
    }

    public async Task<Result<InvoiceStatusDto>> UpdateAsync(
        int id,
        UpdateInvoiceStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var invoiceStatus = await invoiceStatusRepository.GetByIdAsync(id, cancellationToken);

        if (invoiceStatus is null)
        {
            return Result<InvoiceStatusDto>.Failure(InvoiceStatusErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<InvoiceStatusDto>.Failure(validationError);
        }

        var nameAlreadyExists = await invoiceStatusRepository.ExistsAsync(
            x => x.Name == normalizedName && x.InvoiceStatusId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<InvoiceStatusDto>.Failure(InvoiceStatusErrors.NameAlreadyExists);
        }

        invoiceStatus.Name = normalizedName;

        invoiceStatusRepository.Update(invoiceStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceStatusDto>.Success(MapToDto(invoiceStatus));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoiceStatusRepository = _unitOfWork.Repository<InvoiceStatus>();
        var invoiceStatus = await invoiceStatusRepository.GetByIdAsync(id, cancellationToken);

        if (invoiceStatus is null)
        {
            return Result.Failure(InvoiceStatusErrors.NotFound);
        }

        var invoiceRepository = _unitOfWork.Repository<Invoice>();
        var inUse = await invoiceRepository.ExistsAsync(
            x => x.InvoiceStatusId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(InvoiceStatusErrors.InUse);
        }

        invoiceStatusRepository.Remove(invoiceStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static InvoiceStatusDto MapToDto(InvoiceStatus invoiceStatus)
    {
        return new InvoiceStatusDto
        {
            InvoiceStatusId = invoiceStatus.InvoiceStatusId,
            Name = invoiceStatus.Name
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
            return InvoiceStatusErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return InvoiceStatusErrors.NameTooLong;
        }

        return null;
    }
}
