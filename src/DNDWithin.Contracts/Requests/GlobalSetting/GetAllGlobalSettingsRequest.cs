namespace DNDWithin.Contracts.Requests.GlobalSetting;

public class GetAllGlobalSettingsRequest : PagedRequest
{
    public string? Name { get; init; }
    public string? SortBy { get; init; }
}