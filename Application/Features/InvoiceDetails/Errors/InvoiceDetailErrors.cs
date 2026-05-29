using Application.Common.Results;

namespace Application.Features.InvoiceDetails.Errors;

public static class InvoiceDetailErrors
{
    public static readonly Error NotFound = new("InvoiceDetails.NotFound", "Invoice detail was not found.");
    public static readonly Error InvoiceIdInvalid = new("InvoiceDetails.InvoiceIdInvalid", "InvoiceId must be greater than 0.");
    public static readonly Error InvoiceNotFound = new("InvoiceDetails.InvoiceNotFound", "Invoice was not found.");
    public static readonly Error SourcePartIdInvalid = new("InvoiceDetails.SourcePartIdInvalid", "SourcePartId must be greater than 0 when provided.");
    public static readonly Error SourcePartNotFound = new("InvoiceDetails.SourcePartNotFound", "Source part was not found.");
    public static readonly Error ConceptRequired = new("InvoiceDetails.ConceptRequired", "Concept is required.");
    public static readonly Error ConceptTooLong = new("InvoiceDetails.ConceptTooLong", "Concept cannot exceed 150 characters.");
    public static readonly Error QuantityInvalid = new("InvoiceDetails.QuantityInvalid", "Quantity must be greater than 0.");
    public static readonly Error UnitPriceInvalid = new("InvoiceDetails.UnitPriceInvalid", "UnitPrice must be greater than or equal to 0.");
    public static readonly Error LineTypeRequired = new("InvoiceDetails.LineTypeRequired", "LineType is required.");
    public static readonly Error LineTypeTooLong = new("InvoiceDetails.LineTypeTooLong", "LineType cannot exceed 20 characters.");
    public static readonly Error LineTypeInvalid = new("InvoiceDetails.LineTypeInvalid", "LineType must be 'part' or 'labor'.");
    public static readonly Error SourcePartRequiredForPartLine = new("InvoiceDetails.SourcePartRequiredForPartLine", "SourcePartId is required for line type 'part'.");
}
