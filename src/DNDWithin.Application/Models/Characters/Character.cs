using DNDWithin.Application.Models.Characters;

namespace DNDWithin.Application.Models;

public class Character
{
    public required Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public Characteristics Characteristics { get; set; } = new();
}