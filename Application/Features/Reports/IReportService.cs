using Application.Common.Results;
using Application.Features.Reports.Dtos;

namespace Application.Features.Reports;

public interface IReportService
{
    Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<Result<InventoryReportDto>> GetInventoryReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<Result<MechanicsReportDto>> GetMechanicsReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<Result<ServiceOrdersReportDto>> GetServiceOrdersReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<Result<PaymentsReportDto>> GetPaymentsReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
