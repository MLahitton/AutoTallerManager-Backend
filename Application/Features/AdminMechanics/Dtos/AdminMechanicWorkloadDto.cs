namespace Application.Features.AdminMechanics.Dtos;

public class AdminMechanicWorkloadDto
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int AssignedServicesCount { get; set; }
    public int ActiveOrdersCount { get; set; }
    public int PendingServicesCount { get; set; }
    public int InProgressServicesCount { get; set; }
    public int CompletedServicesCount { get; set; }
    public IReadOnlyList<AdminMechanicWorkloadServiceDto> Services { get; set; } = Array.Empty<AdminMechanicWorkloadServiceDto>();
}
