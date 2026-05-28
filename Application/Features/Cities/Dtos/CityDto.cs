namespace Application.Features.Cities.Dtos;

public class CityDto
{
    public int CityId { get; set; }
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
}
