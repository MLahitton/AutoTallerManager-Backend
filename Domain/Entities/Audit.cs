namespace Domain.Entities;

public class Audit
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public int AuditActionTypeId { get; set; }
    public string AffectedEntity { get; set; } = string.Empty;
    public int AffectedRecordId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public AuditActionType AuditActionType { get; set; } = null!;
}
