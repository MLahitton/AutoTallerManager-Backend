namespace Application.Features.Cities.Requests;

public class CreateCityRequest
{
    public int DepartmentId { get; set; }
    public string? Name { get; set; }
}
