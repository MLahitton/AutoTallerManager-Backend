namespace Application.Features.Persons.Dtos;

public class PersonDto
{
    public int PersonId { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? GenderId { get; set; }
    public int? AddressId { get; set; }
    public DateTime CreatedAt { get; set; }
}
