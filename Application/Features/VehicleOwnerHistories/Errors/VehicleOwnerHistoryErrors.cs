using Application.Common.Results;

namespace Application.Features.VehicleOwnerHistories.Errors;

public static class VehicleOwnerHistoryErrors
{
    public static readonly Error NotFound = new("VehicleOwnerHistories.NotFound", "Vehicle owner history record was not found.");
    public static readonly Error VehicleIdInvalid = new("VehicleOwnerHistories.VehicleIdInvalid", "VehicleId must be greater than 0.");
    public static readonly Error VehicleNotFound = new("VehicleOwnerHistories.VehicleNotFound", "Vehicle was not found.");
    public static readonly Error PersonIdInvalid = new("VehicleOwnerHistories.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("VehicleOwnerHistories.PersonNotFound", "Person was not found.");
    public static readonly Error StartDateInvalid = new("VehicleOwnerHistories.StartDateInvalid", "StartDate is invalid.");
    public static readonly Error EndDateInvalid = new("VehicleOwnerHistories.EndDateInvalid", "EndDate is invalid.");
    public static readonly Error CurrentOwnerAlreadyExists = new("VehicleOwnerHistories.CurrentOwnerAlreadyExists", "A current owner already exists for this vehicle.");
    public static readonly Error RelationAlreadyExists = new("VehicleOwnerHistories.RelationAlreadyExists", "This owner relation already exists.");
}
