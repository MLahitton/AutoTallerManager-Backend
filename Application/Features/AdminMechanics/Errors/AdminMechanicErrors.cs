using Application.Common.Results;

namespace Application.Features.AdminMechanics.Errors;

public static class AdminMechanicErrors
{
    public static readonly Error PersonIdInvalid = new("AdminMechanics.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("AdminMechanics.PersonNotFound", "Person was not found.");
    public static readonly Error MechanicNotFound = new("AdminMechanics.MechanicNotFound", "Mechanic was not found.");
}
