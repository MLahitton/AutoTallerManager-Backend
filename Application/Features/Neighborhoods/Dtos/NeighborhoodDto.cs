namespace Application.Features.Neighborhoods.Dtos;

public class NeighborhoodDto
{
    public int NeighborhoodId { get; set; }
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
}
