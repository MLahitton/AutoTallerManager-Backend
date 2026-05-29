using Application.Common.Results;

namespace Application.Features.VehicleEntryInventories.Errors;

public static class VehicleEntryInventoryErrors
{
    public static readonly Error NotFound = new("VehicleEntryInventories.NotFound", "Vehicle entry inventory was not found.");
    public static readonly Error ServiceOrderIdInvalid = new("VehicleEntryInventories.ServiceOrderIdInvalid", "ServiceOrderId must be greater than 0.");
    public static readonly Error ServiceOrderNotFound = new("VehicleEntryInventories.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error ServiceOrderAlreadyExists = new("VehicleEntryInventories.ServiceOrderAlreadyExists", "A vehicle entry inventory already exists for this service order.");
    public static readonly Error ScratchesDescriptionRequired = new("VehicleEntryInventories.ScratchesDescriptionRequired", "ScratchesDescription is required when HasScratches is true.");
    public static readonly Error ToolboxDescriptionRequired = new("VehicleEntryInventories.ToolboxDescriptionRequired", "ToolboxDescription is required when HasToolbox is true.");
}
