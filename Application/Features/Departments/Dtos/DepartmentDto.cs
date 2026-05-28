namespace Application.Features.Departments.Dtos;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
}
