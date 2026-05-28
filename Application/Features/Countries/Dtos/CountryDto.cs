namespace Application.Features.Countries.Dtos;

public class CountryDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
}
