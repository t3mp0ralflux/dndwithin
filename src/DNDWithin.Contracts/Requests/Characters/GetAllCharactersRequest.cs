namespace DNDWithin.Contracts.Requests.Characters;

public class GetAllCharactersRequest : PagedRequest
{
    public string? Name { get; init; }
    public int? Level { get; init; }
    public string? Class { get; init; }
    public string? Species { get; init; }
    public string? SortBy { get; init; }
}