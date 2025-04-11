using Bogus;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Application.Models.System;

namespace Testing.Common;

public static class Fakes
{
    public static Account GenerateAccount(AccountStatus? status = AccountStatus.active, AccountRole? role = AccountRole.admin, string? userName = null, bool isDeleted = false)
    {
        Faker<Account> fakeAccount = new Faker<Account>()
                                     .RuleFor(x => x.Id, f => Guid.NewGuid())
                                     .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                                     .RuleFor(x => x.LastName, f => f.Person.LastName)
                                     .RuleFor(x => x.Username, f => string.IsNullOrWhiteSpace(userName) ? f.Internet.UserName() : userName)
                                     .RuleFor(x => x.Email, f => f.Person.Email)
                                     .RuleFor(x => x.Password, f => f.Internet.Password())
                                     .RuleFor(x => x.AccountStatus, f => status)
                                     .RuleFor(x => x.AccountRole, f => role)
                                     .RuleFor(x => x.CreatedUtc, f => f.Date.Recent())
                                     .RuleFor(x => x.UpdatedUtc, f => f.Date.Recent())
                                     .RuleFor(x => x.LastLoginUtc, f => f.Date.Recent())
                                     .RuleFor(x => x.DeletedUtc, f => isDeleted ? DateTime.UtcNow : null);

        return fakeAccount;
    }

    public static EmailData GenerateEmailData(DateTime? sendAfterUtc = null)
    {
        Faker<EmailData>? fakeSetting = new Faker<EmailData>()
                                        .RuleFor(x => x.Id, _ => Guid.NewGuid())
                                        .RuleFor(x=>x.ShouldSend, _ => true)
                                        .RuleFor(x=>x.SendAttempts, f=> 0)
                                        .RuleFor(x=>x.SendAfterUtc, f=> (sendAfterUtc ??= f.Date.Recent() ))
                                        .RuleFor(x=> x.SenderEmail, f=> f.Person.Email)
                                        .RuleFor(x=> x.RecipientEmail, f=> f.Person.Email)
                                        .RuleFor(x=>x.SenderAccountId, _=>Guid.NewGuid())
                                        .RuleFor(x=>x.ReceiverAccountId, _=>Guid.NewGuid())
                                        .RuleFor(x=>x.ResponseLog, f=>f.System.FileType())
                                        .RuleFor(x=>x.Body, f=> f.Internet.ExampleEmail());

            return fakeSetting;
    }

    public static GlobalSetting GenerateGlobalSetting(string? value = null)
    {
        Faker<GlobalSetting>? fakeSetting = new Faker<GlobalSetting>()
                                            .RuleFor(x => x.Id, _ => Guid.NewGuid())
                                            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
                                            .RuleFor(x => x.Value, f => value ??= f.Hacker.Noun());
        
        return fakeSetting;
    }
    
    public static Character GenerateNewCharacter(Account account)
    {
        Faker<Characteristics> characteristicsFaker = new Faker<Characteristics>()
                                                      .RuleFor(x=>x.Age, _=> "")
                                                      .RuleFor(x=>x.Eyes, _ => "")
                                                      .RuleFor(x=>x.Faith, _ => "")
                                                      .RuleFor(x=>x.Gender, _ => "") 
                                                      .RuleFor(x=>x.Hair, _ => "")
                                                      .RuleFor(x=>x.Height, _ => "")
                                                      .RuleFor(x=>x.Skin, _ => "")
                                                      .RuleFor(x=>x.Weight, _ => "");
        
        Faker<Character>? fakeCharacter = new Faker<Character>()
                                          .RuleFor(x=>x.Id, _ => Guid.NewGuid())
                                          .RuleFor(x=>x.AccountId, _ => account.Id)
                                          .RuleFor(x=>x.Username, _ => account.Username)
                                          .RuleFor(x=>x.Name, f => f.Person.FullName)
                                          .RuleFor(x=>x.Characteristics, _ => characteristicsFaker.Generate(1).First());

        return fakeCharacter;
    }

    public static Character GenerateCharacter(Account account)
    {
        Faker<Characteristics> characteristicsFaker = new Faker<Characteristics>()
                                                       .RuleFor(x=>x.Age, f=> f.Random.Number(1,99).ToString())
                                                       .RuleFor(x=>x.Eyes, f => f.Commerce.Color())
                                                       .RuleFor(x=>x.Faith, _ => "") // this is NOT a slight against any religion, just checking that it accepts an empty value.
                                                       .RuleFor(x=>x.Gender, f => f.Random.Word()) // this is NOT a slight against anyone, just checking that it accepts all text.
                                                       .RuleFor(x=>x.Hair, f => f.Commerce.Color())
                                                       .RuleFor(x=>x.Height, _ => "69cm or 420' 0\"") // checking for rando stuff people could put in there.
                                                       .RuleFor(x=>x.Skin, f => f.Commerce.Color())
                                                       .RuleFor(x=>x.Weight, f => f.Random.Number(69, 420).ToString());
        
        Faker<Character>? fakeCharacter = new Faker<Character>()
                                          .RuleFor(x=>x.Id, _ => Guid.NewGuid())
                                          .RuleFor(x=>x.AccountId, _ => account.Id)
                                          .RuleFor(x=>x.Username, _ => account.Username)
                                          .RuleFor(x=>x.Name, f => f.Person.FullName)
                                          .RuleFor(x=>x.Characteristics, _ => characteristicsFaker.Generate(1).First());

        return fakeCharacter;
    }
}