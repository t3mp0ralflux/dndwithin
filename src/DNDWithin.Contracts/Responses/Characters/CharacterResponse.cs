namespace DNDWithin.Contracts.Responses.Characters;

public class CharacterResponse
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public string Gender { get; set; }
    public string Age { get; set; }
    public string Hair { get; set; }
    public string Eyes { get; set; }
    public string Skin { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
}