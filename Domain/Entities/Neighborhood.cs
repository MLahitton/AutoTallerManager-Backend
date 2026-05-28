namespace Domain.Entities;

public class Neighborhood
{
    public int NeighborhoodId { get; set; }
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;

    public City City { get; set; } = null!;
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
