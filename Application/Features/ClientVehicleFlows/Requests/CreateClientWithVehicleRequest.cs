namespace Application.Features.ClientVehicleFlows.Requests;

public class CreateClientWithVehicleRequest
{
    public int DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
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
    public int ModelId { get; set; }
    public int VehicleTypeId { get; set; }
    public string? VIN { get; set; }
    public int Year { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
}
