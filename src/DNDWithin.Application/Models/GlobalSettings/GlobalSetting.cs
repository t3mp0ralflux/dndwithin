namespace DNDWithin.Application.Models.GlobalSettings;

public class GlobalSetting
{
    public Guid Id { get; init; }
    public string Name { get; set; }
    public string Value { get; set; }
}