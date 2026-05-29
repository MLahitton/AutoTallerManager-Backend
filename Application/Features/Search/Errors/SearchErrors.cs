using Application.Common.Results;

namespace Application.Features.Search.Errors;

public static class SearchErrors
{
    public static readonly Error SearchTermRequired = new("Search.SearchTermRequired", "Search term is required.");
    public static readonly Error SearchTermTooShort = new("Search.SearchTermTooShort", "Search term must be at least 2 characters.");
}
