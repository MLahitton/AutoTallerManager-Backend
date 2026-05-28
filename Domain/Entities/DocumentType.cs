namespace Domain.Entities;

public class DocumentType
{
    public int DocumentTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Person> Persons { get; set; } = new List<Person>();
}
