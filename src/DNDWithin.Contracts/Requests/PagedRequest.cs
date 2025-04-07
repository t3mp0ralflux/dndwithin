namespace DNDWithin.Contracts.Requests;

public class PagedRequest
{
    public required int Page { get; set; } = 1;
    public required int PageSize { get; set; } = 10;
}