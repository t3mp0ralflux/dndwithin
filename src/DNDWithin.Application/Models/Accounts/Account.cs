﻿namespace DNDWithin.Application.Models.Accounts;

public class Account
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Username { get; init; }
    public string? Email { get; set; }
    public string? Password { get; set; }

    // managed by API
    public AccountStatus AccountStatus { get; set; }
    public AccountRole AccountRole { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public DateTime LastLoginUtc { get; set; }
    public DateTime? DeletedUtc { get; set; }
}