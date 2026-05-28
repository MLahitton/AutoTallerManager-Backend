namespace Application.Features.PersonEmails.Requests;

public class CreatePersonEmailRequest
{
    public int PersonId { get; set; }
    public int EmailDomainId { get; set; }
    public string? EmailUser { get; set; }
    public bool IsPrimary { get; set; }
}
