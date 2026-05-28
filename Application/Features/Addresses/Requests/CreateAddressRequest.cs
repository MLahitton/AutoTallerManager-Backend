namespace Application.Features.Addresses.Requests;

public class CreateAddressRequest
{
    public int NeighborhoodId { get; set; }
    public int StreetTypeId { get; set; }
    public string? MainNumber { get; set; }
    public string? SecondaryNumber { get; set; }
    public string? TertiaryNumber { get; set; }
    public string? Complement { get; set; }
}
