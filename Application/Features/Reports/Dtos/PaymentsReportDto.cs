namespace Application.Features.Reports.Dtos;

public class PaymentsReportDto
{
    public int TotalPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CompletedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal PendingOrOtherAmount { get; set; }
}
