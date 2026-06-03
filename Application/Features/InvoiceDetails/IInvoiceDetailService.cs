using Application.Common.Results;
using Application.Features.InvoiceDetails.Dtos;
using Application.Features.InvoiceDetails.Requests;

namespace Application.Features.InvoiceDetails;

public interface IInvoiceDetailService
{
    Task<Result<IReadOnlyList<InvoiceDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<InvoiceDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<InvoiceDetailsByInvoiceDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);

    Task<Result<InvoiceDetailDto>> CreateAsync(CreateInvoiceDetailRequest request, CancellationToken cancellationToken = default);

    Task<Result<InvoiceDetailDto>> UpdateAsync(int id, UpdateInvoiceDetailRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
