using Application.Common.Results;

namespace Application.Features.Dashboards.Errors;

public static class DashboardErrors
{
    public static readonly Error CurrentPersonIdInvalid = new("Dashboards.CurrentPersonIdInvalid", "Current person id must be greater than 0.");
    public static readonly Error PersonNotFound = new("Dashboards.PersonNotFound", "Person was not found.");
    public static readonly Error PersonIsNotClientInvalid = new("Dashboards.PersonIsNotClientInvalid", "Person does not have an active client role.");
    public static readonly Error PersonIsNotMechanicInvalid = new("Dashboards.PersonIsNotMechanicInvalid", "Person does not have an active mechanic role.");
}
