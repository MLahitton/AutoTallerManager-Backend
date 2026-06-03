using Application.Common.Results;
using Application.Features.Suppliers.Dtos;
using Application.Features.Suppliers.Requests;

namespace Application.Features.Suppliers;

public interface ISupplierService
{
    Task<Result<IReadOnlyList<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<SupplierDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result<SupplierDto>> UpdateAsync(int id, UpdateSupplierRequest request, int currentUserId, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
}
