﻿namespace DNDWithin.Application.Models;

public class Account
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    
    // managed by API
    public AccountStatus AccountStatus { get; set; }
    public AccountRole AccountRole { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public DateTime LastLoginUtc { get; set; }
    public DateTime? DeletedUtc { get; set; }
}