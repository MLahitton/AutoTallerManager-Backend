namespace Domain.Entities;

public class Gender
{
    public int GenderId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Person> Persons { get; set; } = new List<Person>();
}
