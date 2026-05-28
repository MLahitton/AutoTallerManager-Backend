namespace Application.Features.Neighborhoods.Requests;

public class CreateNeighborhoodRequest
{
    public int CityId { get; set; }
    public string? Name { get; set; }
}
