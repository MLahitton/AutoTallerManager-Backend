using Application.Common.Results;

namespace Application.Features.Reports.Errors;

public static class ReportErrors
{
    public static readonly Error DateRangeInvalid = new("Reports.DateRangeInvalid", "From date cannot be greater than To date.");
}
