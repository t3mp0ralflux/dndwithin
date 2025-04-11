namespace DNDWithin.Contracts.Responses.Characters;

public class GetAllCharacterResponse
{
    public required Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
}