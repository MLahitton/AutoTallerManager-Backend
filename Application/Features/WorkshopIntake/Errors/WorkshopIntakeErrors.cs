using Application.Common.Results;

namespace Application.Features.WorkshopIntake.Errors;

public static class WorkshopIntakeErrors
{
    public static readonly Error VehicleIdInvalid = new("WorkshopIntake.VehicleIdInvalid", "VehicleId must be greater than 0.");
    public static readonly Error VehicleNotFound = new("WorkshopIntake.VehicleNotFound", "Vehicle was not found.");
    public static readonly Error VehicleInactive = new("WorkshopIntake.VehicleInactive", "Vehicle is inactive.");
    public static readonly Error ActiveOrderAlreadyExistsConflict = new("WorkshopIntake.ActiveOrderAlreadyExistsConflict", "Vehicle already has an active service order.");
    public static readonly Error InitialOrderStatusIdInvalid = new("WorkshopIntake.InitialOrderStatusIdInvalid", "InitialOrderStatusId must be greater than 0.");
    public static readonly Error InitialOrderStatusNotFound = new("WorkshopIntake.InitialOrderStatusNotFound", "Initial order status was not found.");
    public static readonly Error InitialOrderStatusInvalidConflict = new("WorkshopIntake.InitialOrderStatusInvalidConflict", "Initial order status cannot be Cancelled or Voided.");
    public static readonly Error PendingStatusNotFound = new("WorkshopIntake.PendingStatusNotFound", "Pending status was not found.");
    public static readonly Error EntryDateInvalid = new("WorkshopIntake.EntryDateInvalid", "EntryDate is invalid.");
    public static readonly Error EstimatedDeliveryDateInvalid = new("WorkshopIntake.EstimatedDeliveryDateInvalid", "EstimatedDeliveryDate must be greater than or equal to EntryDate.");
    public static readonly Error ScratchesDescriptionRequired = new("WorkshopIntake.ScratchesDescriptionRequired", "ScratchesDescription is required when HasScratches is true.");
    public static readonly Error ToolboxDescriptionRequired = new("WorkshopIntake.ToolboxDescriptionRequired", "ToolboxDescription is required when HasToolbox is true.");
    public static readonly Error ServiceTypeIdInvalid = new("WorkshopIntake.ServiceTypeIdInvalid", "ServiceTypeId must be greater than 0.");
    public static readonly Error ServiceTypeNotFound = new("WorkshopIntake.ServiceTypeNotFound", "Service type was not found.");
    public static readonly Error LaborCostInvalid = new("WorkshopIntake.LaborCostInvalid", "LaborCost must be greater than or equal to 0.");
    public static readonly Error ChangedByUserIdInvalid = new("WorkshopIntake.ChangedByUserIdInvalid", "ChangedByUserId must be greater than 0.");
    public static readonly Error ChangedByUserNotFound = new("WorkshopIntake.ChangedByUserNotFound", "Changed-by user was not found.");
}
