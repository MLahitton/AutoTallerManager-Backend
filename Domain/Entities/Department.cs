namespace Domain.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; set; } = new List<City>();
}
