namespace Application.Features.AuditQueries.Dtos;

public class AuditQueryDto
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public int AuditActionTypeId { get; set; }
    public string AffectedEntity { get; set; } = string.Empty;
    public int AffectedRecordId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
