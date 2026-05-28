namespace Application.Features.PersonPhones.Dtos;

public class PersonPhoneDto
{
    public int PersonPhoneId { get; set; }
    public int PersonId { get; set; }
    public int CountryId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
