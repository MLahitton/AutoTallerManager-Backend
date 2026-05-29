namespace Application.Features.ServiceOrderWorkflow.Dtos;

public class ServiceOrderPaymentSummaryDto
{
    public int PaymentId { get; set; }
    public int PaymentMethodId { get; set; }
    public int PaymentStatusId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}
