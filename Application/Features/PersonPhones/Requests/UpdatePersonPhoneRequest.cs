namespace Application.Features.PersonPhones.Requests;

public class UpdatePersonPhoneRequest
{
    public int PersonId { get; set; }
    public int CountryId { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsPrimary { get; set; }
}
