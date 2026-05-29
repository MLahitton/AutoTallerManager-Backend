using Application.Common.Results;
using Application.Features.InvoiceBusiness.Dtos;
using Application.Features.InvoiceBusiness.Requests;

namespace Application.Features.InvoiceBusiness;

public interface IInvoiceBusinessService
{
    Task<Result<GeneratedInvoiceDto>> GenerateFromServiceOrderAsync(int serviceOrderId, GenerateInvoiceFromServiceOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<InvoiceBusinessResultDto>> RecalculateAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<Result<InvoiceBusinessResultDto>> IssueAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<Result<InvoiceBusinessResultDto>> CancelAsync(int invoiceId, CancelInvoiceRequest request, CancellationToken cancellationToken = default);
}
