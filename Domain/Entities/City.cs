namespace Domain.Entities;

public class City
{
    public int CityId { get; set; }
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Department Department { get; set; } = null!;
    public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
}
