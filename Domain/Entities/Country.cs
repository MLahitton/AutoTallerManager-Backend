namespace Domain.Entities;

public class Country
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }

    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<PersonPhone> PersonPhones { get; set; } = new List<PersonPhone>();
}
