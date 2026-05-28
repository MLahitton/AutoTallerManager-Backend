namespace Domain.Entities;

public class PersonPhone
{
    public int PersonPhoneId { get; set; }
    public int PersonId { get; set; }
    public int CountryId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Person Person { get; set; } = null!;
    public Country Country { get; set; } = null!;
}
