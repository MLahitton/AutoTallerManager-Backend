namespace Application.Features.Audits.Requests;

public class UpdateAuditRequest
{
    public int UserId { get; set; }
    public int AuditActionTypeId { get; set; }
    public string? AffectedEntity { get; set; }
    public int AffectedRecordId { get; set; }
    public string? Description { get; set; }
}
