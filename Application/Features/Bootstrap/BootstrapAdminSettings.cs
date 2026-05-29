namespace Application.Features.Bootstrap;

public sealed class BootstrapAdminSettings
{
    public bool Enabled { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public int DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? SecondLastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? GenderId { get; set; }
    public int? AddressId { get; set; }
}
