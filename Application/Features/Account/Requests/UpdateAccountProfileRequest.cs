namespace Application.Features.Account.Requests;

public class UpdateAccountProfileRequest
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? SecondLastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? GenderId { get; set; }
    public int? AddressId { get; set; }
    public string? Email { get; set; }
    public int? PhoneCountryId { get; set; }
    public string? PhoneNumber { get; set; }
}
