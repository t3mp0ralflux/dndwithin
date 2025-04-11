namespace DNDWithin.Contracts.Responses.Characters;

public class CharacterResponse
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Gender { get; set; }
    public required string Age { get; set; }
    public required string Hair { get; set; }
    public required string Eyes { get; set; }
    public required string Skin { get; set; }
    public required string Height { get; set; }
    public required string Weight { get; set; }
    public required string Faith { get; set; }
}