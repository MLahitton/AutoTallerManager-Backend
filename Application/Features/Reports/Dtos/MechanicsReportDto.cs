namespace Application.Features.Reports.Dtos;

public class MechanicsReportDto
{
    public int TotalMechanics { get; set; }
    public int ActiveMechanics { get; set; }
    public int TotalAssignments { get; set; }
    public int ServicesWithWorkPerformed { get; set; }
    public int ServicesPendingWorkPerformed { get; set; }
}
