using Application.Common.Results;

namespace Application.Features.ServiceOrderWorkflow.Errors;

public static class ServiceOrderWorkflowErrors
{
    public static readonly Error NotFound = new("ServiceOrderWorkflow.NotFound", "Service order was not found.");
    public static readonly Error ServiceOrderIdInvalid = new("ServiceOrderWorkflow.ServiceOrderIdInvalid", "ServiceOrderId must be greater than 0.");
    public static readonly Error NewOrderStatusIdInvalid = new("ServiceOrderWorkflow.NewOrderStatusIdInvalid", "NewOrderStatusId must be greater than 0.");
    public static readonly Error NewOrderStatusNotFound = new("ServiceOrderWorkflow.NewOrderStatusNotFound", "New order status was not found.");
    public static readonly Error ChangedByUserIdInvalid = new("ServiceOrderWorkflow.ChangedByUserIdInvalid", "ChangedByUserId must be greater than 0.");
    public static readonly Error ChangedByUserNotFound = new("ServiceOrderWorkflow.ChangedByUserNotFound", "Changed-by user was not found.");
    public static readonly Error ClientCannotAccessServiceOrderConflict = new("ServiceOrderWorkflow.ClientCannotAccessServiceOrderConflict", "Client cannot access this service order.");
    public static readonly Error MechanicCannotAccessServiceOrderConflict = new("ServiceOrderWorkflow.MechanicCannotAccessServiceOrderConflict", "Mechanic cannot access this service order.");
    public static readonly Error PreviousOrderStatusNotFound = new("ServiceOrderWorkflow.PreviousOrderStatusNotFound", "Previous order status was not found.");
    public static readonly Error ServiceOrderCannotBeCancelledConflict = new("ServiceOrderWorkflow.ServiceOrderCannotBeCancelledConflict", "Service order cannot be cancelled.");
    public static readonly Error ServiceOrderCannotBeVoidedConflict = new("ServiceOrderWorkflow.ServiceOrderCannotBeVoidedConflict", "Service order cannot be voided.");
    public static readonly Error ServiceOrderCannotBeCompletedConflict = new("ServiceOrderWorkflow.ServiceOrderCannotBeCompletedConflict", "Service order cannot be completed.");
    public static readonly Error CancelReasonRequired = new("ServiceOrderWorkflow.CancelReasonRequired", "Cancel/Void reason is required.");
    public static readonly Error CancelledStatusNotFound = new("ServiceOrderWorkflow.CancelledStatusNotFound", "Cancelled status was not found.");
    public static readonly Error VoidedStatusNotFound = new("ServiceOrderWorkflow.VoidedStatusNotFound", "Voided status was not found.");
    public static readonly Error CompletedStatusNotFound = new("ServiceOrderWorkflow.CompletedStatusNotFound", "Completed status was not found.");
}
