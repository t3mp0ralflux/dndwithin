namespace DNDWithin.Contracts.Requests.Characters;

public class CharacterUpdateRequest
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Gender { get; init; }
    public required string Age { get; init; }
    public required string Hair { get; init; }
    public required string Eyes { get; init; }
    public required string Skin { get; init; }
    public required string Height { get; init; }
    public required string Weight { get; init; }
    public required string Faith { get; set; }
}