namespace Domain.Entities;

public class StreetType
{
    public int StreetTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
