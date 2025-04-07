namespace DNDWithin.Application.Models.GlobalSettings;

public class GetAllGlobalSettingsOptions
{
    public string? Name { get; set; }
    public string? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}