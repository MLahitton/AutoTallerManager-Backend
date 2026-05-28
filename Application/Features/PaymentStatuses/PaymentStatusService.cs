using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PaymentStatuses.Dtos;
using Application.Features.PaymentStatuses.Errors;
using Application.Features.PaymentStatuses.Requests;
using Domain.Entities;

namespace Application.Features.PaymentStatuses;

public class PaymentStatusService : IPaymentStatusService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentStatusService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PaymentStatusDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatuses = await paymentStatusRepository.GetAllAsync(cancellationToken);

        var paymentStatusDtos = paymentStatuses
            .OrderBy(x => x.PaymentStatusId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PaymentStatusDto>>.Success(paymentStatusDtos);
    }

    public async Task<Result<PaymentStatusDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatus = await paymentStatusRepository.GetByIdAsync(id, cancellationToken);

        if (paymentStatus is null)
        {
            return Result<PaymentStatusDto>.Failure(PaymentStatusErrors.NotFound);
        }

        return Result<PaymentStatusDto>.Success(MapToDto(paymentStatus));
    }

    public async Task<Result<PaymentStatusDto>> CreateAsync(
        CreatePaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PaymentStatusDto>.Failure(validationError);
        }

        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var nameAlreadyExists = await paymentStatusRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PaymentStatusDto>.Failure(PaymentStatusErrors.NameAlreadyExists);
        }

        var paymentStatus = new PaymentStatus
        {
            Name = normalizedName
        };

        await paymentStatusRepository.AddAsync(paymentStatus, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentStatusDto>.Success(MapToDto(paymentStatus));
    }

    public async Task<Result<PaymentStatusDto>> UpdateAsync(
        int id,
        UpdatePaymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatus = await paymentStatusRepository.GetByIdAsync(id, cancellationToken);

        if (paymentStatus is null)
        {
            return Result<PaymentStatusDto>.Failure(PaymentStatusErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PaymentStatusDto>.Failure(validationError);
        }

        var nameAlreadyExists = await paymentStatusRepository.ExistsAsync(
            x => x.Name == normalizedName && x.PaymentStatusId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PaymentStatusDto>.Failure(PaymentStatusErrors.NameAlreadyExists);
        }

        paymentStatus.Name = normalizedName;

        paymentStatusRepository.Update(paymentStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentStatusDto>.Success(MapToDto(paymentStatus));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentStatusRepository = _unitOfWork.Repository<PaymentStatus>();
        var paymentStatus = await paymentStatusRepository.GetByIdAsync(id, cancellationToken);

        if (paymentStatus is null)
        {
            return Result.Failure(PaymentStatusErrors.NotFound);
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        var inUse = await paymentRepository.ExistsAsync(
            x => x.PaymentStatusId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(PaymentStatusErrors.InUse);
        }

        paymentStatusRepository.Remove(paymentStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PaymentStatusDto MapToDto(PaymentStatus paymentStatus)
    {
        return new PaymentStatusDto
        {
            PaymentStatusId = paymentStatus.PaymentStatusId,
            Name = paymentStatus.Name
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
            return PaymentStatusErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return PaymentStatusErrors.NameTooLong;
        }

        return null;
    }
}
