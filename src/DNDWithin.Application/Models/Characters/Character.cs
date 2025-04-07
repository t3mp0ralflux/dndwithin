namespace DNDWithin.Application.Models.Characters;

public class Character
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required string Username { get; init; }
    public required string Name { get; set; }
    public Characteristics Characteristics { get; set; } = new();
}