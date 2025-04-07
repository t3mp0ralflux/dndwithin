namespace DNDWithin.Application.Models;

public class SharedGetAllOptions
{
    public string? SortField { get; init; }
    public SortOrder? SortOrder { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}