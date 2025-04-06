namespace DNDWithin.Application.Models.GlobalSettings;

public class GlobalSetting
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Value { get; set; }
}