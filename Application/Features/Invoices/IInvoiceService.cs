using Application.Common.Results;
using Application.Features.Invoices.Dtos;
using Application.Features.Invoices.Requests;

namespace Application.Features.Invoices;

public interface IInvoiceService
{
    Task<Result<IReadOnlyList<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<InvoiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<InvoiceDto>> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);

    Task<Result<InvoiceDto>> UpdateAsync(int id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
