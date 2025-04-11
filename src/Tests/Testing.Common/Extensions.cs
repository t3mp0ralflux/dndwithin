using DNDWithin.Application.Models.Accounts;

namespace Testing.Common;

public static class Extensions
{
    public static Account WithActivation(this Account account)
    {
        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.Now;

        return account;
    }
}