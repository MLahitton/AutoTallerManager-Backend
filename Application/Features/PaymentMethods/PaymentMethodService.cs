using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.PaymentMethods.Dtos;
using Application.Features.PaymentMethods.Errors;
using Application.Features.PaymentMethods.Requests;
using Domain.Entities;

namespace Application.Features.PaymentMethods;

public class PaymentMethodService : IPaymentMethodService
{
    private const int NameMaxLength = 50;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentMethodService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PaymentMethodDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethods = await paymentMethodRepository.GetAllAsync(cancellationToken);

        var paymentMethodDtos = paymentMethods
            .OrderBy(x => x.PaymentMethodId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<PaymentMethodDto>>.Success(paymentMethodDtos);
    }

    public async Task<Result<PaymentMethodDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod is null)
        {
            return Result<PaymentMethodDto>.Failure(PaymentMethodErrors.NotFound);
        }

        return Result<PaymentMethodDto>.Success(MapToDto(paymentMethod));
    }

    public async Task<Result<PaymentMethodDto>> CreateAsync(
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PaymentMethodDto>.Failure(validationError);
        }

        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var nameAlreadyExists = await paymentMethodRepository.ExistsAsync(
            x => x.Name == normalizedName,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PaymentMethodDto>.Failure(PaymentMethodErrors.NameAlreadyExists);
        }

        var paymentMethod = new PaymentMethod
        {
            Name = normalizedName
        };

        await paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentMethodDto>.Success(MapToDto(paymentMethod));
    }

    public async Task<Result<PaymentMethodDto>> UpdateAsync(
        int id,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod is null)
        {
            return Result<PaymentMethodDto>.Failure(PaymentMethodErrors.NotFound);
        }

        var normalizedName = NormalizeName(request?.Name);
        var validationError = ValidateName(normalizedName);
        if (validationError is not null)
        {
            return Result<PaymentMethodDto>.Failure(validationError);
        }

        var nameAlreadyExists = await paymentMethodRepository.ExistsAsync(
            x => x.Name == normalizedName && x.PaymentMethodId != id,
            cancellationToken);

        if (nameAlreadyExists)
        {
            return Result<PaymentMethodDto>.Failure(PaymentMethodErrors.NameAlreadyExists);
        }

        paymentMethod.Name = normalizedName;

        paymentMethodRepository.Update(paymentMethod);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentMethodDto>.Success(MapToDto(paymentMethod));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var paymentMethodRepository = _unitOfWork.Repository<PaymentMethod>();
        var paymentMethod = await paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod is null)
        {
            return Result.Failure(PaymentMethodErrors.NotFound);
        }

        var paymentRepository = _unitOfWork.Repository<Payment>();
        var inUse = await paymentRepository.ExistsAsync(
            x => x.PaymentMethodId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(PaymentMethodErrors.InUse);
        }

        paymentMethodRepository.Remove(paymentMethod);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PaymentMethodDto MapToDto(PaymentMethod paymentMethod)
    {
        return new PaymentMethodDto
        {
            PaymentMethodId = paymentMethod.PaymentMethodId,
            Name = paymentMethod.Name
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
            return PaymentMethodErrors.NameRequired;
        }

        if (name.Length > NameMaxLength)
        {
            return PaymentMethodErrors.NameTooLong;
        }

        return null;
    }
}
