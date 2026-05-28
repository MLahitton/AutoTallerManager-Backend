namespace Application.Common.Pagination;

public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public PaginationRequest()
    {
    }

    public PaginationRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : value;
    }
}
