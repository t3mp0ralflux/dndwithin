namespace DNDWithin.Contracts.Requests.GlobalSetting;

public class GlobalSettingCreateRequest<T>
{
    public required string Name { get; set; }
    public required T Value { get; set; }
}