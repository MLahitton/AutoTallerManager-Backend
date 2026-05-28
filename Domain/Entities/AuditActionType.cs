namespace Domain.Entities;

public class AuditActionType
{
    public int AuditActionTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Audit> Audits { get; set; } = new List<Audit>();
}
