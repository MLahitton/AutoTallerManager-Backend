namespace Application.Features.PersonEmails.Dtos;

public class PersonEmailDto
{
    public int PersonEmailId { get; set; }
    public int PersonId { get; set; }
    public int EmailDomainId { get; set; }
    public string EmailUser { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
