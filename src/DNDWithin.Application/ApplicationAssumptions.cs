namespace DNDWithin.Application;

public static class ApplicationAssumptions
{
    /// <summary>
    /// 0: Username
    /// 1: Activation Code
    /// </summary>
    public const string ACTIVATION_LINK_FORMAT = "https://localhost:5001/api/accounts/activate/{0}/{1}";

    /// <summary>
    /// 0: Username
    /// 1: Activation Code
    /// </summary>
    public const string RESEND_ACTIVATION_LINK_FORMAT = "https://localhost:5001/api/account/activate/{0}/{1}/resend";
}