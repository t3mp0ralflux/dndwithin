namespace DNDWithin.Application.Models.GlobalSettings;

public class GetAllGlobalSettingsOptions
{
    public string? Name { get; set; }
    public string? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}