namespace Application.Features.Account.Dtos;

public class AccountProfileDto
{
    public int UserId { get; set; }
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
    public string? PrimaryEmail { get; set; }
    public int? PrimaryPhoneCountryId { get; set; }
    public string? PrimaryPhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}
