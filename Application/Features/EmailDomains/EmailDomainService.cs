using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.EmailDomains.Dtos;
using Application.Features.EmailDomains.Errors;
using Application.Features.EmailDomains.Requests;
using Domain.Entities;

namespace Application.Features.EmailDomains;

public class EmailDomainService : IEmailDomainService
{
    private const int DomainMaxLength = 100;
    private readonly IUnitOfWork _unitOfWork;

    public EmailDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<EmailDomainDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomains = await emailDomainRepository.GetAllAsync(cancellationToken);

        var emailDomainDtos = emailDomains
            .OrderBy(x => x.EmailDomainId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<EmailDomainDto>>.Success(emailDomainDtos);
    }

    public async Task<Result<EmailDomainDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomain = await emailDomainRepository.GetByIdAsync(id, cancellationToken);

        if (emailDomain is null)
        {
            return Result<EmailDomainDto>.Failure(EmailDomainErrors.NotFound);
        }

        return Result<EmailDomainDto>.Success(MapToDto(emailDomain));
    }

    public async Task<Result<EmailDomainDto>> CreateAsync(CreateEmailDomainRequest request, CancellationToken cancellationToken = default)
    {
        var domain = NormalizeDomain(request?.Domain);
        var validationError = ValidateDomain(domain);

        if (validationError is not null)
        {
            return Result<EmailDomainDto>.Failure(validationError);
        }

        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var alreadyExists = await emailDomainRepository.ExistsAsync(
            x => x.Domain == domain,
            cancellationToken);

        if (alreadyExists)
        {
            return Result<EmailDomainDto>.Failure(EmailDomainErrors.DomainAlreadyExists);
        }

        var emailDomain = new EmailDomain
        {
            Domain = domain
        };

        await emailDomainRepository.AddAsync(emailDomain, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmailDomainDto>.Success(MapToDto(emailDomain));
    }

    public async Task<Result<EmailDomainDto>> UpdateAsync(int id, UpdateEmailDomainRequest request, CancellationToken cancellationToken = default)
    {
        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomain = await emailDomainRepository.GetByIdAsync(id, cancellationToken);

        if (emailDomain is null)
        {
            return Result<EmailDomainDto>.Failure(EmailDomainErrors.NotFound);
        }

        var domain = NormalizeDomain(request?.Domain);
        var validationError = ValidateDomain(domain);

        if (validationError is not null)
        {
            return Result<EmailDomainDto>.Failure(validationError);
        }

        var alreadyExists = await emailDomainRepository.ExistsAsync(
            x => x.Domain == domain && x.EmailDomainId != id,
            cancellationToken);

        if (alreadyExists)
        {
            return Result<EmailDomainDto>.Failure(EmailDomainErrors.DomainAlreadyExists);
        }

        emailDomain.Domain = domain;

        emailDomainRepository.Update(emailDomain);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmailDomainDto>.Success(MapToDto(emailDomain));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var emailDomainRepository = _unitOfWork.Repository<EmailDomain>();
        var emailDomain = await emailDomainRepository.GetByIdAsync(id, cancellationToken);

        if (emailDomain is null)
        {
            return Result.Failure(EmailDomainErrors.NotFound);
        }

        var personEmailRepository = _unitOfWork.Repository<PersonEmail>();
        var inUse = await personEmailRepository.ExistsAsync(
            x => x.EmailDomainId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(EmailDomainErrors.InUse);
        }

        emailDomainRepository.Remove(emailDomain);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static EmailDomainDto MapToDto(EmailDomain emailDomain)
    {
        return new EmailDomainDto
        {
            EmailDomainId = emailDomain.EmailDomainId,
            Domain = emailDomain.Domain
        };
    }

    private static string NormalizeDomain(string? domain)
    {
        return (domain ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static Error? ValidateDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return EmailDomainErrors.DomainRequired;
        }

        if (domain.Length > DomainMaxLength)
        {
            return EmailDomainErrors.DomainTooLong;
        }

        if (domain.Contains('@') || !domain.Contains('.'))
        {
            return EmailDomainErrors.DomainInvalid;
        }

        return null;
    }
}
