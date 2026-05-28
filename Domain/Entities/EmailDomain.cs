namespace Domain.Entities;

public class EmailDomain
{
    public int EmailDomainId { get; set; }
    public string Domain { get; set; } = string.Empty;

    public ICollection<PersonEmail> PersonEmails { get; set; } = new List<PersonEmail>();
}
