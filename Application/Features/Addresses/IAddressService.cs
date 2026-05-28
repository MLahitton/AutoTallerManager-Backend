using Application.Common.Results;
using Application.Features.Addresses.Dtos;
using Application.Features.Addresses.Requests;

namespace Application.Features.Addresses;

public interface IAddressService
{
    Task<Result<IReadOnlyList<AddressDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<AddressDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<AddressDto>> CreateAsync(CreateAddressRequest request, CancellationToken cancellationToken = default);

    Task<Result<AddressDto>> UpdateAsync(int id, UpdateAddressRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
