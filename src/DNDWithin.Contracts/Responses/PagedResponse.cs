namespace DNDWithin.Contracts.Responses;

public class PagedResponse<TResponse>
{
    public required IEnumerable<TResponse> Items { get; set; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; set; }
    public bool HasNextPage => Total > Page * PageSize;
}