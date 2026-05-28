using Application.Common.Results;
using Application.Features.InvoiceStatuses.Dtos;
using Application.Features.InvoiceStatuses.Requests;

namespace Application.Features.InvoiceStatuses;

public interface IInvoiceStatusService
{
    Task<Result<IReadOnlyList<InvoiceStatusDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<InvoiceStatusDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<InvoiceStatusDto>> CreateAsync(CreateInvoiceStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result<InvoiceStatusDto>> UpdateAsync(int id, UpdateInvoiceStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
