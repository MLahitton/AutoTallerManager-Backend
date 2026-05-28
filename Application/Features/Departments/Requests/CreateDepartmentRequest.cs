namespace Application.Features.Departments.Requests;

public class CreateDepartmentRequest
{
    public int CountryId { get; set; }
    public string? Name { get; set; }
}
