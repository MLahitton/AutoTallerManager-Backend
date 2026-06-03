using Application.Common.Results;

namespace Application.Features.ClientApprovals.Errors;

public static class ClientApprovalErrors
{
    public static readonly Error InvalidPersonId = new("ClientApprovals.InvalidPersonId", "Current person id must be greater than 0.");
    public static readonly Error OrderServiceIdInvalid = new("ClientApprovals.OrderServiceIdInvalid", "OrderServiceId must be greater than 0.");
    public static readonly Error OrderServicePartIdInvalid = new("ClientApprovals.OrderServicePartIdInvalid", "OrderServicePartId must be greater than 0.");
    public static readonly Error OrderServiceNotFound = new("ClientApprovals.OrderServiceNotFound", "Order service was not found.");
    public static readonly Error OrderServicePartNotFound = new("ClientApprovals.OrderServicePartNotFound", "Order service part was not found.");
    public static readonly Error ServiceOrderNotFound = new("ClientApprovals.ServiceOrderNotFound", "Service order was not found.");
    public static readonly Error NotOwner = new("ClientApprovals.NotOwnerForbidden", "Client cannot access this approval item.");
    public static readonly Error AlreadyApproved = new("ClientApprovals.AlreadyApprovedConflict", "Approval item is already approved.");
    public static readonly Error AlreadyRejected = new("ClientApprovals.AlreadyRejectedConflict", "Approval item is already rejected.");
    public static readonly Error ApprovalAlreadyDecided = new("ClientApprovals.ApprovalAlreadyDecidedConflict", "Approval item has already been decided.");
}
