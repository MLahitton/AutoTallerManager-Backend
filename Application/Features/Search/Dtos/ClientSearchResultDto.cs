namespace Application.Features.Search.Dtos;

public class ClientSearchResultDto
{
    public int PersonId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhoneNumber { get; set; }
}
