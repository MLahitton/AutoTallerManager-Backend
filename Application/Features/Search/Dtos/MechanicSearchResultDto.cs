namespace Application.Features.Search.Dtos;

public class MechanicSearchResultDto
{
    public int PersonId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IReadOnlyList<int> SpecialtyIds { get; set; } = Array.Empty<int>();
}
