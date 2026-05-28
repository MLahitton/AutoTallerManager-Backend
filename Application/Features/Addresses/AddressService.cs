using Application.Common.Interfaces.Persistence;
using Application.Common.Results;
using Application.Features.Addresses.Dtos;
using Application.Features.Addresses.Errors;
using Application.Features.Addresses.Requests;
using Domain.Entities;

namespace Application.Features.Addresses;

public class AddressService : IAddressService
{
    private const int MainNumberMaxLength = 10;
    private const int SecondaryNumberMaxLength = 10;
    private const int TertiaryNumberMaxLength = 10;
    private const int ComplementMaxLength = 150;
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AddressDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var addressRepository = _unitOfWork.Repository<Address>();
        var addresses = await addressRepository.GetAllAsync(cancellationToken);

        var addressDtos = addresses
            .OrderBy(x => x.AddressId)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<AddressDto>>.Success(addressDtos);
    }

    public async Task<Result<AddressDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var addressRepository = _unitOfWork.Repository<Address>();
        var address = await addressRepository.GetByIdAsync(id, cancellationToken);

        if (address is null)
        {
            return Result<AddressDto>.Failure(AddressErrors.NotFound);
        }

        return Result<AddressDto>.Success(MapToDto(address));
    }

    public async Task<Result<AddressDto>> CreateAsync(
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var neighborhoodId = request?.NeighborhoodId ?? 0;
        var streetTypeId = request?.StreetTypeId ?? 0;
        var normalizedMainNumber = NormalizeOptional(request?.MainNumber);
        var normalizedSecondaryNumber = NormalizeOptional(request?.SecondaryNumber);
        var normalizedTertiaryNumber = NormalizeOptional(request?.TertiaryNumber);
        var normalizedComplement = NormalizeOptional(request?.Complement);

        var validationError = Validate(
            neighborhoodId,
            streetTypeId,
            normalizedMainNumber,
            normalizedSecondaryNumber,
            normalizedTertiaryNumber,
            normalizedComplement);

        if (validationError is not null)
        {
            return Result<AddressDto>.Failure(validationError);
        }

        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var neighborhoodExists = await neighborhoodRepository.ExistsAsync(
            x => x.NeighborhoodId == neighborhoodId,
            cancellationToken);

        if (!neighborhoodExists)
        {
            return Result<AddressDto>.Failure(AddressErrors.NeighborhoodNotFound);
        }

        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var streetTypeExists = await streetTypeRepository.ExistsAsync(
            x => x.StreetTypeId == streetTypeId,
            cancellationToken);

        if (!streetTypeExists)
        {
            return Result<AddressDto>.Failure(AddressErrors.StreetTypeNotFound);
        }

        var address = new Address
        {
            NeighborhoodId = neighborhoodId,
            StreetTypeId = streetTypeId,
            MainNumber = normalizedMainNumber,
            SecondaryNumber = normalizedSecondaryNumber,
            TertiaryNumber = normalizedTertiaryNumber,
            Complement = normalizedComplement
        };

        var addressRepository = _unitOfWork.Repository<Address>();
        await addressRepository.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AddressDto>.Success(MapToDto(address));
    }

    public async Task<Result<AddressDto>> UpdateAsync(
        int id,
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var addressRepository = _unitOfWork.Repository<Address>();
        var address = await addressRepository.GetByIdAsync(id, cancellationToken);

        if (address is null)
        {
            return Result<AddressDto>.Failure(AddressErrors.NotFound);
        }

        var neighborhoodId = request?.NeighborhoodId ?? 0;
        var streetTypeId = request?.StreetTypeId ?? 0;
        var normalizedMainNumber = NormalizeOptional(request?.MainNumber);
        var normalizedSecondaryNumber = NormalizeOptional(request?.SecondaryNumber);
        var normalizedTertiaryNumber = NormalizeOptional(request?.TertiaryNumber);
        var normalizedComplement = NormalizeOptional(request?.Complement);

        var validationError = Validate(
            neighborhoodId,
            streetTypeId,
            normalizedMainNumber,
            normalizedSecondaryNumber,
            normalizedTertiaryNumber,
            normalizedComplement);

        if (validationError is not null)
        {
            return Result<AddressDto>.Failure(validationError);
        }

        var neighborhoodRepository = _unitOfWork.Repository<Neighborhood>();
        var neighborhoodExists = await neighborhoodRepository.ExistsAsync(
            x => x.NeighborhoodId == neighborhoodId,
            cancellationToken);

        if (!neighborhoodExists)
        {
            return Result<AddressDto>.Failure(AddressErrors.NeighborhoodNotFound);
        }

        var streetTypeRepository = _unitOfWork.Repository<StreetType>();
        var streetTypeExists = await streetTypeRepository.ExistsAsync(
            x => x.StreetTypeId == streetTypeId,
            cancellationToken);

        if (!streetTypeExists)
        {
            return Result<AddressDto>.Failure(AddressErrors.StreetTypeNotFound);
        }

        address.NeighborhoodId = neighborhoodId;
        address.StreetTypeId = streetTypeId;
        address.MainNumber = normalizedMainNumber;
        address.SecondaryNumber = normalizedSecondaryNumber;
        address.TertiaryNumber = normalizedTertiaryNumber;
        address.Complement = normalizedComplement;

        addressRepository.Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AddressDto>.Success(MapToDto(address));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var addressRepository = _unitOfWork.Repository<Address>();
        var address = await addressRepository.GetByIdAsync(id, cancellationToken);

        if (address is null)
        {
            return Result.Failure(AddressErrors.NotFound);
        }

        var personRepository = _unitOfWork.Repository<Person>();
        var inUse = await personRepository.ExistsAsync(
            x => x.AddressId == id,
            cancellationToken);

        if (inUse)
        {
            return Result.Failure(AddressErrors.InUse);
        }

        addressRepository.Remove(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static AddressDto MapToDto(Address address)
    {
        return new AddressDto
        {
            AddressId = address.AddressId,
            NeighborhoodId = address.NeighborhoodId,
            StreetTypeId = address.StreetTypeId,
            MainNumber = address.MainNumber,
            SecondaryNumber = address.SecondaryNumber,
            TertiaryNumber = address.TertiaryNumber,
            Complement = address.Complement
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static Error? Validate(
        int neighborhoodId,
        int streetTypeId,
        string? mainNumber,
        string? secondaryNumber,
        string? tertiaryNumber,
        string? complement)
    {
        if (neighborhoodId <= 0)
        {
            return AddressErrors.NeighborhoodIdInvalid;
        }

        if (streetTypeId <= 0)
        {
            return AddressErrors.StreetTypeIdInvalid;
        }

        if (mainNumber is not null && mainNumber.Length > MainNumberMaxLength)
        {
            return AddressErrors.MainNumberTooLong;
        }

        if (secondaryNumber is not null && secondaryNumber.Length > SecondaryNumberMaxLength)
        {
            return AddressErrors.SecondaryNumberTooLong;
        }

        if (tertiaryNumber is not null && tertiaryNumber.Length > TertiaryNumberMaxLength)
        {
            return AddressErrors.TertiaryNumberTooLong;
        }

        if (complement is not null && complement.Length > ComplementMaxLength)
        {
            return AddressErrors.ComplementTooLong;
        }

        return null;
    }
}
