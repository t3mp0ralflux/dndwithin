namespace DNDWithin.Contracts.Responses.GlobalSetting;

public class GlobalSettingResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; init; }
    public required string Value { get; init; }
}