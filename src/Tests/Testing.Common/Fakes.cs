using Bogus;
using DNDWithin.Application.Models.Accounts;

namespace Testing.Common;

public static class Fakes
{
    public static Account GenerateAccount(AccountStatus status = AccountStatus.active, AccountRole role = AccountRole.admin, bool isDeleted = false)
    {
        Faker<Account> fakeAccount = new Faker<Account>()
                                     .RuleFor(x => x.Id, f => Guid.NewGuid())
                                     .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                                     .RuleFor(x => x.LastName, f => f.Person.LastName)
                                     .RuleFor(x => x.UserName, f => f.Internet.UserName())
                                     .RuleFor(x => x.Email, f => f.Person.Email)
                                     .RuleFor(x => x.Password, f => f.Internet.Password())
                                     .RuleFor(x=>x.AccountStatus, f=> status)
                                     .RuleFor(x=>x.AccountRole, f=> role)
                                     .RuleFor(x=>x.CreatedUtc, f=>f.Date.Recent())
                                     .RuleFor(x=>x.UpdatedUtc, f=>f.Date.Recent())
                                     .RuleFor(x=>x.LastLoginUtc, f=>f.Date.Recent())
                                     .RuleFor(x=>x.DeletedUtc, f=> isDeleted ? DateTime.UtcNow : null);

        return fakeAccount;
    }
}