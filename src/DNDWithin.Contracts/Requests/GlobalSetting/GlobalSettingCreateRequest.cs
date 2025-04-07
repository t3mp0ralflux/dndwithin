namespace DNDWithin.Contracts.Requests.GlobalSetting;

public class GlobalSettingCreateRequest
{
    public required string Name { get; set; }
    public required string Value { get; set; }
}