namespace Domain.Entities;

public class PersonEmail
{
    public int PersonEmailId { get; set; }
    public int PersonId { get; set; }
    public int EmailDomainId { get; set; }
    public string EmailUser { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Person Person { get; set; } = null!;
    public EmailDomain EmailDomain { get; set; } = null!;
}
